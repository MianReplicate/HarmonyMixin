using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HarmonyMixinBootstrap
{
 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Mixin : Attribute
    {
        public Type ClassType;
        
        public Mixin(Type classType)
        {
            ClassType = classType;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class BaseModification : Attribute
    {
        public readonly InjectionType InjectionType;
        public readonly Shift Shift;
        public readonly string MethodInvokedIn;
        public readonly MethodInfo TargetMethod;
        public readonly int Ordinal;

        public MethodInfo MixinMethod;

        protected static BaseModification Context;
        
        protected BaseModification(string methodInvokedIn, InjectionType injectionType, Shift shift, Type typeTargetMethodIn, string targetMethodName, int ordinal = 0)
        {
            MethodInvokedIn = methodInvokedIn;
            TargetMethod = AccessTools.Method(typeTargetMethodIn, targetMethodName);
            Ordinal = ordinal;
            Shift = shift;
            InjectionType = injectionType;
        }

        public abstract bool VerifyIsValid(MethodInfo mixinMethod);

        public void PatchMethod(Harmony harmony, Type classMethodInvokedIn)
        {
            if (MixinMethod == null)
                return;

            Internal_PatchMethod(harmony, classMethodInvokedIn);
            // harmony.Patch(AccessTools.Method(classMethodInvokedIn, MethodInvokedIn),  transpiler: AccessTools.Method(inheritedType, nameof(Internal_PatchMethod)));
        }
        
        protected abstract void Internal_PatchMethod(Harmony harmony, Type classMethodInvokedIn);

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Inject : Attribute
    {
        public readonly InjectionType InjectionType;
        public readonly string MethodToInjectIn;
        public readonly string TargetMethod;
    
        public Inject(InjectionType injectionType, string methodToInjectIn, string targetMethod)
        {
            InjectionType = injectionType;
            MethodToInjectIn = methodToInjectIn;
            TargetMethod = targetMethod;
        }
    }

    public class RedirectMethod : BaseModification
    {
        public RedirectMethod(string methodInvokedIn, Type typeTargetMethodIn, string targetMethodName, int ordinal = 0) : base(methodInvokedIn, InjectionType.Invoke, Shift.Before, typeTargetMethodIn, targetMethodName, ordinal)
        {
            
        }

        public override bool VerifyIsValid(MethodInfo mixinMethod)
        {
            if (TargetMethod.ReturnType != mixinMethod.ReturnType)
            {
                LoggerUtility.Info($"Cannot redirect {TargetMethod.Name} to {mixinMethod.Name} because they have different return types!");
                return false;
            }
                
            var origParameters = TargetMethod.GetParameters();
            var mixinParameters = mixinMethod.GetParameters();
                
            if (mixinParameters.Length - 1 > origParameters.Length || mixinParameters.Length <= origParameters.Length)
            {
                LoggerUtility.Info($"Cannot redirect {TargetMethod.Name} to {mixinMethod.Name} because the mixin method does not have the same parameters!");
                return false;
            }

                
            for (var i = 0; i < origParameters.Length; i++)
            {
                if (origParameters[i].ParameterType != mixinParameters[i + 1].ParameterType)
                {
                    LoggerUtility.Info($"Cannot redirect {TargetMethod.Name} to {mixinMethod.Name} because they have different parameter types!");
                    return false;
                }
            }

            MixinMethod = mixinMethod;
            return true;
        }

        protected override void Internal_PatchMethod(Harmony harmony, Type classMethodInvokedIn)
        {
            Context = this;
            harmony.Patch(AccessTools.Method(classMethodInvokedIn, MethodInvokedIn),  transpiler: AccessTools.Method(typeof(RedirectMethod), nameof(ActualPatch)));
        }

        protected static IEnumerable<CodeInstruction> ActualPatch(IEnumerable<CodeInstruction> instructions)
        {
            var currentOrdinal = 0;
            var inserted = false;

            foreach (var instruction in instructions)
            {
                if (inserted)
                {
                    yield return instruction;
                    goto Skipped;
                }
                if (!instruction.Calls(Context.TargetMethod)){
                    yield return instruction;
                    goto Skipped;
                }
                if (currentOrdinal != Context.Ordinal)
                {
                    currentOrdinal += 1;
                    yield return instruction;
                    goto Skipped;
                }

                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RedirectMethod), nameof(MiddleMan)));
                inserted = true;
                    
                Skipped: ;
            }
        }

        #nullable enable
        private static object? MiddleMan(params object[] grr)
        {
            return null;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Local : Attribute
    {
        public readonly Type VariableType;
        public readonly int Ordinal;
    
        public Local(Type variableType, int ordinal = 0)
        {
            VariableType = variableType;
            Ordinal = ordinal;
        }
    }
}
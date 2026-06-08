using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace HarmonyMixinBootstrap.MixinTypes
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
    public abstract class BaseModification<T> : Attribute where T : BaseModification<T>
    {
        public readonly InjectionType InjectionType;
        public readonly Shift Shift;
        public readonly string MethodInvokedIn;
        public readonly MethodInfo TargetMethod;
        public readonly int Ordinal;
        public readonly int Priority;
    
        public MethodInfo MixinMethod;
    
        protected static BaseModification<T> Context;
        
        protected BaseModification(string methodInvokedIn, InjectionType injectionType, Shift shift, Type typeTargetMethodIn, string targetMethodName, int ordinal = 0, int priority = 500)
        {
            MethodInvokedIn = methodInvokedIn;
            TargetMethod = AccessTools.Method(typeTargetMethodIn, targetMethodName);
            Ordinal = ordinal;
            Shift = shift;
            InjectionType = injectionType;
            Priority = priority;
        }

        protected void BringCursorToInjectionPoint(CodeMatcher matcher)
        {
            
        }
    
        public abstract bool VerifyIsValid(MethodInfo mixinMethod);
    
        public void PatchMethod(Harmony harmony, Type classMethodInvokedIn)
        {
            if (MixinMethod == null)
                return;
    
            Internal_PatchMethod(harmony, classMethodInvokedIn);
        }
        
        protected abstract void Internal_PatchMethod(Harmony harmony, Type classMethodInvokedIn);
        protected abstract List<T> GetRegisteredModifications();
    }
    
    // [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    // public class Inject : Attribute
    // {
    //     public readonly InjectionType InjectionType;
    //     public readonly string MethodToInjectIn;
    //     public readonly string TargetMethod;
    //
    //     public Inject(InjectionType injectionType, string methodToInjectIn, string targetMethod)
    //     {
    //         InjectionType = injectionType;
    //         MethodToInjectIn = methodToInjectIn;
    //         TargetMethod = targetMethod;
    //     }
    // }
    //
    // [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    // public class Local : Attribute
    // {
    //     public readonly Type VariableType;
    //     public readonly int Ordinal;
    //
    //     public Local(Type variableType, int ordinal = 0)
    //     {
    //         VariableType = variableType;
    //         Ordinal = ordinal;
    //     }
    // }
}
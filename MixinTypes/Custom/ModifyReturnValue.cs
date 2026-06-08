using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HarmonyMixinBootstrap.MixinTypes.Custom;

public class ModifyReturnValue : BaseModification<ModifyReturnValue>
{
    protected static List<ModifyReturnValue> Modifications = new();
    
    public ModifyReturnValue(string methodInvokedIn, Type typeTargetMethodIn, string targetMethodName, int ordinal = 0, int priority = 500) : base(methodInvokedIn, InjectionType.Invoke, Shift.Before, typeTargetMethodIn, targetMethodName, ordinal, priority)
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
    private static object? MiddleMan(params object[] args)
    {
        var highestPriority = int.MaxValue;
        RedirectMethod annotationToUse = null;
        foreach (var redirectMethod in Modifications)
        {
            if (redirectMethod.Priority < highestPriority)
            {
                highestPriority = redirectMethod.Priority;
                annotationToUse = redirectMethod;
            }
        }

        return annotationToUse.MixinMethod.Invoke(null, args);
    }

    protected override List<RedirectMethod> GetRegisteredModifications()
    {
        return Modifications;
    }
}
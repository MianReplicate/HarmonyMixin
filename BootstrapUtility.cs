using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HarmonyMixinBootstrap
{
 
public static class BootstrapUtility
{
        private static void MoveCursorToBeginningOfMethod(this CodeMatcher matcher, MethodInfo method, int ordinal)
        {
            
        }

        private static void MoveCursorToEndingOfMethod(this CodeMatcher matcher, MethodInfo method, int ordinal)
        {
            
        }
        
        // // How much is currently on the stack at this index
        // private static Dictionary<int, int> GetInstructionToStackCount(List<CodeInstruction> instructions, int targetIndex)
        // {
        //     var stack = 0;
        //
        //     for (int i = 0; i < instructions.Count && i < targetIndex; i++)
        //     {
        //         var code = instructions[i];
        //         var op = code.opcode;
        //
        //         // PUSH instructions
        //         if (code.IsLoad())
        //         {
        //             stack++;
        //         }
        //         else if (op == OpCodes.Dup)
        //         {
        //             stack++;
        //         }
        //
        //         // POP instructions
        //         else if (op == OpCodes.Pop)
        //         {
        //             stack--;
        //         }
        //
        //         // STORE (pop one value)
        //         else if (code.IsStore())
        //         {
        //             stack--;
        //             if (op == OpCodes.Stfld)
        //                 stack--;
        //         }
        //
        //         // CALL handling
        //         else if (op == OpCodes.Call || op == OpCodes.Callvirt)
        //         {
        //             var method = code.operand as MethodInfo;
        //             if (method != null)
        //             {
        //                 int pops = method.GetParameters().Length;
        //
        //                 // instance method => consumes "this"
        //                 if (!method.IsStatic)
        //                     pops++;
        //
        //                 stack -= pops;
        //
        //                 if (method.ReturnType != typeof(void))
        //                     stack++;
        //             }
        //         }
        //
        //         // safety clamp (helps debug bad IL assumptions)
        //         if (stack < 0)
        //             throw new Exception($"Negative stack at IL index {i}: {code}");
        //     }
        //
        //     return stack;
        // }
        
        
        // private static bool IsLoad(this CodeInstruction instruction)
        // {
        //     var op = instruction.opcode;
        //     return instruction.IsLdarg() || instruction.IsLdarga() || instruction.IsLdloc() || instruction.LoadsConstant() || op == OpCodes.Ldfld || op == OpCodes.Ldsflda || op == OpCodes.Ldstr;
        // }
        //
        // private static bool IsStore(this CodeInstruction instruction)
        // {
        //     return instruction.IsStarg() || instruction.IsStloc() || instruction.opcode == OpCodes.Stsfld || instruction.opcode == OpCodes.Stfld;
        // }
    }   
}
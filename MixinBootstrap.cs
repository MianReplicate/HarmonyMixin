using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HarmonyMixinBootstrap
{
     // ModifyVariable
    // ModifyReturnValue
    // Inject (Before & After)
    // RedirectMethod
    // WrapMethod

    // difficult implementations
    // WrapOperation (wraps calls in a method)
    // maybe Local if I'm feeling it but that's very difficult
    public class MixinBootstrap
    {
        private const string ContextParamName = "context";
        
        private static MixinBootstrap _instance;
        
        private Harmony _harmony;
        private Assembly _assembly;
        private static readonly Dictionary<MethodBase, List<KeyValuePair<MethodInfo, Inject>>> _injectionRegistry = new Dictionary<MethodBase, List<KeyValuePair<MethodInfo, Inject>>>();
        private static readonly Dictionary<MethodBase, KeyValuePair<MethodInfo, RedirectMethod>> _redirectRegistry = new Dictionary<MethodBase, KeyValuePair<MethodInfo, RedirectMethod>>();
        
        public MixinBootstrap(Harmony harmony, Assembly assembly)
        {
            _instance = this;
            _harmony = harmony;
            _assembly = assembly;
        }

        public void Unregister()
        {
            _injectionRegistry.Clear();
            _redirectRegistry.Clear();
        }
        
        public void Register()
        {
            LoggerUtility.Info("Registering mixin bootstrap!");
            
            foreach (var type in _assembly.GetTypes())
            {
                var mixin = type.GetCustomAttribute<Mixin>();
                if (mixin != null)
                {
                    foreach (var method in AccessTools.GetDeclaredMethods(type))
                    {
                        if (!method.IsStatic)
                        {
                            LoggerUtility.Info($"{method.Name} must be static in order to use it for mixins!");
                            continue;
                        }
                        
                        // RegisterInjectIfOneExists(mixin.ClassType, method);
                        HandleRedirect(mixin.ClassType, method);
                    }   
                }
            }
        }

        // private void RegisterInjectIfOneExists(Type classType, MethodInfo mixinMethod)
        // {
        //     var injectAttribute = mixinMethod.GetCustomAttribute<Inject>();
        //     if(injectAttribute != null)
        //     {
        //         var methodToInject = AccessTools.Method(classType, injectAttribute.MethodToInjectIn);
        //
        //         if (!_injectionRegistry.TryGetValue(methodToInject, out var listOfInjects))
        //         {
        //             listOfInjects = new List<KeyValuePair<MethodInfo, Inject>>();
        //             _injectionRegistry.Add(methodToInject, listOfInjects);
        //             _harmony.Patch(methodToInject, transpiler: new HarmonyMethod(typeof(MixinBootstrap).GetMethod(nameof(InjectTranspiler))));
        //         }
        //         
        //         listOfInjects.Add(new KeyValuePair<MethodInfo, Inject>(mixinMethod, injectAttribute));
        //     }
        // }
        
        private void HandleRedirect(Type classMethodInvokedIn, MethodInfo mixinMethod)
        {
            var redirectAttribute = mixinMethod.GetCustomAttribute<RedirectMethod>();
            if(redirectAttribute != null)
            {
                if(redirectAttribute.VerifyIsValid(mixinMethod))
                    redirectAttribute.PatchMethod(_harmony, classMethodInvokedIn);
            }
        }

        // public static IEnumerable<CodeInstruction> RedirectTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        // {
        //     if (_redirectRegistry.TryGetValue(original, out var pair))
        //     {
        //         var mixinMethod = pair.Key;
        //         var redirectAnnotation = pair.Value;
        //         var wantedIndex = redirectAnnotation.Ordinal;
        //         var currentIndex = 0;
        //         var inserted = false;
        //
        //         foreach (var instruction in instructions)
        //         {
        //             if (inserted)
        //             {
        //                 yield return instruction;
        //                 goto Skipped;
        //             }
        //             if (!instruction.Calls(redirectAnnotation.MethodToRedirect)){
        //                 yield return instruction;
        //                 goto Skipped;
        //             }
        //             if (currentIndex != wantedIndex)
        //             {
        //                 currentIndex += 1;
        //                 yield return instruction;
        //                 goto Skipped;
        //             }
        //
        //             yield return new CodeInstruction(OpCodes.Call, mixinMethod);
        //             inserted = true;
        //             
        //             Skipped: ;
        //         }
        //     }
        //     else
        //     {
        //         LoggerUtility.Info($"Failed to redirect {original.Name}! Couldn't find redirection metadata for it.");
        //         foreach (var instruction in instructions)
        //         {
        //             yield return instruction;
        //         }
        //     }
        // }
        //
        // private static IEnumerable<CodeInstruction> InjectTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase original)
        // {
        //     if (_injectionRegistry.TryGetValue(original, out var metadatas))
        //     {
        //         var methodsToLook = metadatas.Select(metadata => AccessTools.Method(original.DeclaringType, metadata.Value.TargetMethod)).ToList();
        //         foreach (var instruction in instructions)
        //         {
        //             var methodInfo = methodsToLook.Where(instruction.Calls).First();
        //             if (methodInfo == null)
        //                 yield return instruction;
        //             
        //             
        //         }
        //     }
        //     else
        //     {
        //         LoggerUtility.Info($"Failed to inject into {original.Name}! Couldn't find injection metadata for it.");
        //         foreach (var instruction in instructions)
        //         {
        //             yield return instruction;
        //         }
        //     }
        // }
        //
        // private static void RunBeforeInjects()
        // {
        //     
        // }
        //
        // private static void RunAfterInjects()
        // {
        //     
        // }
    }   
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using HarmonyMixinBootstrap.MixinTypes;
using HarmonyMixinBootstrap.MixinTypes.Custom;

namespace HarmonyMixinBootstrap
{
    // TODO: Mixins should always be able to be provided context if applicable (object of method that is invoked) and the instance of the object that the mixin is running in
    // TODO: Create a method that brings cursor to intended injection point for easy patching
    
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
        
        private Harmony _harmony;
        private Assembly _assembly;
        
        public MixinBootstrap(Harmony harmony, Assembly assembly)
        {
            _harmony = harmony;
            _assembly = assembly;
        }
        
        ///<summary>
        /// Due to restrictions imposed by Harmony, this will unpatch all methods globally! You should only be using this method for hot-reloading purposes anyway.
        /// </summary>
        public void Unregister()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }
        
        ///<summary>
        /// Will register every annotation in the given assembly.
        /// </summary>
        public void Register()
        {
            LoggerUtility.Info($"Registering mixins for {_harmony.Id}!");
            
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

                        foreach (var attribute in method.GetCustomAttributes())
                        {
                            var baseType = attribute.GetType().BaseType;
                            if (baseType.Name == typeof(BaseModification<>).Name)
                            {
                                var verifyIsValid = AccessTools.Method(attribute.GetType(), "VerifyIsValid");
                                var patchMethod = AccessTools.Method(attribute.GetType(), "PatchMethod");
                                if((bool) verifyIsValid.Invoke(attribute, new[]{method}))
                                    patchMethod.Invoke(attribute, new object[] {_harmony, mixin.ClassType});
                            }
                        }
                    }   
                }
            }
        }


        private void HandleRedirect(Type classMethodInvokedIn, MethodInfo mixinMethod)
        {
            var redirectAttribute = mixinMethod.GetCustomAttribute<RedirectMethod>();
            if(redirectAttribute != null)
            {
                if(redirectAttribute.VerifyIsValid(mixinMethod))
                    redirectAttribute.PatchMethod(_harmony, classMethodInvokedIn);
            }
        }
    }   
}
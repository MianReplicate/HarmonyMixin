using System;
using System.Reflection;
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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RedirectMethod : Attribute
    {
        public readonly string MethodToInjectIn;
        public readonly MethodInfo MethodToRedirect;
        public readonly int Ordinal;
    
        public RedirectMethod(string methodToInjectIn, Type targetMethodClass, string targetMethod, int ordinal = 0)
        {
            MethodToInjectIn = methodToInjectIn;
            MethodToRedirect = AccessTools.Method(targetMethodClass, targetMethod);
            Ordinal = ordinal;
        }
    }   
}
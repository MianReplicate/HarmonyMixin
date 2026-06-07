using HarmonyLib;

namespace HarmonyMixinBootstrap;

public struct At
{
    InjectionType InjectionType;
    Shift Shift;
    MethodType TargetMethod;
    int Ordinal;
}
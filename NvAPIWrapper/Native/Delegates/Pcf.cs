using System;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.Helpers;

namespace NvAPIWrapper.Native.Delegates
{
    internal static class Pcf
    {
        [FunctionId(FunctionId.NvAPI_PCF_GlobalUnload)]
        public delegate Status NvAPI_PCF_GlobalUnload();

        [FunctionId(FunctionId.NvAPI_PCF_Mode1Initialize)]
        public delegate Status NvAPI_PCF_Mode1Initialize(int mode);

        [FunctionId(FunctionId.NvAPI_PCF_Mode1Unload)]
        public delegate Status NvAPI_PCF_Mode1Unload(int mode);

        [FunctionId(FunctionId.NvAPI_PCF_GetMasterInfo)]
        public delegate Status NvAPI_PCF_GetMasterInfo(IntPtr masterInfoBuffer);

        [FunctionId(FunctionId.NvAPI_PCF_ControllerGetControl)]
        public delegate Status NvAPI_PCF_ControllerGetControl(IntPtr controllerBuffer);

        [FunctionId(FunctionId.NvAPI_PCF_ControllerSetControl)]
        public delegate Status NvAPI_PCF_ControllerSetControl(IntPtr controllerBuffer);

        [FunctionId(FunctionId.NvAPI_PCF_DynamicBoostGetStatus)]
        public delegate Status NvAPI_PCF_DynamicBoostGetStatus(IntPtr statusBuffer);

        [FunctionId(FunctionId.NvAPI_PCF_DynamicBoostSetStatus)]
        public delegate Status NvAPI_PCF_DynamicBoostSetStatus(byte enabled);
    }
}

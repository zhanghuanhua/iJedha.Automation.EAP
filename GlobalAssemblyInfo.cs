/* ==============================
 * 张明哲提醒
 * 全局程序集信息
 * GlobalAssemblyInfo.cs
 * 请把此文件引用到其他的项目中
 ==============================*/

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct("iJDEAP")]
[assembly: AssemblyCompany("Jedha System Eng. Corp.")]
[assembly: AssemblyVersion(RevisionClass.FullVersion)]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCopyright("Copyright ? 2020 Jedha All rights reserved")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


internal static class RevisionClass//EAP版本号
{
    public const string Major = "3";
    public const string Minor = "2021";
    public const string Build = "0901";
    public const string Revision = "1";
    public const string MainVersion = Major + Minor;
    public const string FullVersion = Major + "." + Minor + "." + Build + "." + Revision;
}



/*

其他程序集的AssemblyInfo.cs简化如下内容
所有信息数据单独填写

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("程序集标题")]
[assembly: AssemblyDescription("程序集描述")]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("6a6263f2-35d2-4077-a1aa-cc775ca7cf84")]

[assembly: CLSCompliant(true)]
[assembly: StringFreezing()]
*/


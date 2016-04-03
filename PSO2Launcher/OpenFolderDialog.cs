using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DogStar
{
    public sealed class OpenFolderDialog : CommonDialog
    {
        public string Title { get; set; }
        public string RootFolder { get; set; }
        public string SelectedPath { get; set; }

        [DllImport("shell32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int SHCreateItemFromParsingName(string pszPath, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        public OpenFolderDialog()
        {
            Reset();
        }

        public override void Reset()
        {
            Title = null;
            RootFolder = null;
            SelectedPath = null;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            IFileDialog fileDialog = (IFileDialog)new FileOpenDialogRCW();
            uint options;
            IShellItem resultItem;
            string szName;

            fileDialog.GetOptions(out options);
            options |= 0x2010160;
            fileDialog.SetOptions(options);

            if (!string.IsNullOrEmpty(RootFolder))
            {
                IShellItem folderItem;
                if (SHCreateItemFromParsingName(RootFolder, IntPtr.Zero, Guid.Parse("43826D1E-E718-42EE-BC55-A1E261C37BFE"), out folderItem) == 0L)
                {
                    fileDialog.SetFolder(folderItem);
                }
            }

            fileDialog.SetTitle(Title);

            if (fileDialog.Show(hwndOwner) == 0 && fileDialog.GetResult(out resultItem) == 0 && resultItem.GetDisplayName(0x80058000, out szName) == 0)
            {
                SelectedPath = szName;
                return true;
            }

            return false;
        }

        [ClassInterface(ClassInterfaceType.None)]
        [TypeLibType(TypeLibTypeFlags.FCanCreate)]
        [ComImport, Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        internal class FileOpenDialogRCW
        {
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport, Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
        internal interface IFileDialog
        {
            [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Show(IntPtr hwndOwner);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileTypes(uint cFileTypes, [MarshalAs(UnmanagedType.LPArray)] IntPtr rgFilterSpec);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileTypeIndex(uint iFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetFileTypeIndex(out uint piFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Advise([MarshalAs(UnmanagedType.Interface)] IntPtr pfde, out uint pdwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Unadvise(uint dwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetOptions(uint fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetOptions(out uint fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultFolder([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFolder([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint AddPlace([MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint fdap);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Close([MarshalAs(UnmanagedType.Error)] uint hr);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetClientGuid(ref Guid guid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint ClearClientData();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }

        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint BindToHandler(IntPtr pbc, ref Guid rbhid, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IntPtr ppvOut);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetDisplayName(uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Compare([MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint hint, out int piOrder);
        }
    }
}

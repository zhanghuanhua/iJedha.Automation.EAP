using System;
using System.Windows.Forms;

namespace iJedha.Automation.EAP.UI
{
    static class Program
    {
        //                                          _oo0oo_
        //                                        o8888888o
        //                                        88"   .   "88
        //                                       (  👁     👁 )
        //                                        \      👃     /
        //                                        o\    👄    /o
        //                                            /` - - - '\
        //                                   ——              ——
        //                              .  '    \ \ |             | / /    ` . 
        //                           /      \ \ | | |      :      | | | / /     \  
        //                          /     _| | | | |     -:-     | | | | |_      \    
        //                          |       |   \ \ \      -      / / /   |        | 
        //                          |   \ _|    '  ' \  - - -  / '  '     |_ /    |
        //                          \      .- \__     ` - '        ___/ - .  /
        //                      ___` .  . '     / - - . - - \       ` .   .   __
        //                . " "    '<    ` . __ \_< | >_/ __    .  '    > ' " " .
        //              |   |    :     ` -  \ ` .; ` \   -   / ` ;  . `/   -   `  :    |   |   
        //             \      \    ` - .     \_  __ \    / __  _/       . -    /      /
        // ====== ` - . ____  - . __ \______/ __ . - ` ____ . - ' ======
        //                                        ` = - - - = '
        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //                    佛祖保佑                   永无BUG
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            try
            {
                if (args.Length == 1)
                {
                    string[] arrays = args[0].Split(';');
                    if (arrays[3] == "Develop")
                    {
                        Application.Run(new frmMain("Develop"));
                    }
                    else if (arrays[3] == "Test" || arrays[3] == "Production")
                    {
                        Application.Run(new frmMain(arrays[0], arrays[1], arrays[2], arrays[3]));
                    }
                }
                else
                {
                    MessageBox.Show("EAP启动失败: 启动参数异常.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                System.Environment.Exit(0);
            }
        }
    }
}

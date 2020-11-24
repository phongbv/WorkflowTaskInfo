using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
namespace WorkflowTaskInfo
{
    public partial class Form1 : Form
    {

        public Dispatcher Dispatcher { get; private set; }
        private static Timer myTimer = new Timer();
        public Form1()
        {
            InitializeComponent();
            this.Dispatcher = Dispatcher.CurrentDispatcher;
            ComboBox.ObjectCollection items = this.cbEnv.Items;
            object[] items2 = (from e in this.connectionConfiguration.ConnectionConfigurations
                               select e.Name).ToArray();
            items.AddRange(items2);
            this.cbEnv.SelectedIndex = 0;
            Form1.myTimer.Tick += this.button1_Click;
            Form1.myTimer.Interval = 20000;
            Form1.myTimer.Enabled = true;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Form1.myTimer.Enabled = false;

            }
        }
        private const string SQL_SELECT_TASK = "SELECT U.ORGANIZATIONAL_UNIT_NAME OU, W.*\r\n  FROM WFTASK W, BPM_ORGANIZATIONAL_UNIT U\r\n WHERE     (   PROTECTEDTEXTATTRIBUTE11 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE12 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE13 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE14 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE15 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE16 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE17 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE18 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE19 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE20 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE1 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE2 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE3 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE4 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE5 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE6 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE7 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE8 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE9 LIKE :CARCODE\r\n            OR PROTECTEDTEXTATTRIBUTE10 LIKE :CARCODE)\r\n       AND U.ORGANIZATIONAL_UNIT_ID = W.ORGANIZATIONALUNITID";

        // Token: 0x04000008 RID: 8
        private const string SQL_CHECK_USER_ASSIGN = "SELECT SU.USER_ID\r\n  FROM CAR_STAGE cs, CAR, SYS_USER SU\r\n WHERE     cs.APPLICATION_ID = CAR.ID\r\n       AND CAR.APPLICATION_CODE = :CARCODE\r\n       AND CAR.AMND_STATE = 'F'\r\n       AND cs.ACTION IS NULL\r\n       AND cs.ASSIGNEE_ID = SU.ID(+)";

        // Token: 0x04000009 RID: 9
        private const string SQL_GET_USER_BY_ROLE_AND_OU = "SELECT SU.USER_ID\r\n  FROM SYS_ROLE  SR,\r\n       SYS_USER_ROLE      SUR,\r\n       DEPARTMENT         D,\r\n       SYS_USER           SU\r\n WHERE     UPPER (SR.EXT_REF_NO_1) = UPPER (:role)\r\n       AND SR.AMND_STATE = 'F'\r\n       AND D.DEPT_ID = :ou\r\n       AND SUR.AMND_STATE = 'F'\r\n       AND D.AMND_STATE = 'F'\r\n       AND SUR.ROLE_ID = SR.ID\r\n       AND D.ID = SUR.DEPT_ID\r\n       AND SUR.USER_ID = SU.ID\r\n       AND SU.AMND_STATE = 'F'";

        // Token: 0x0400000A RID: 10
        private ConnectionConfiguration connectionConfiguration = ConnectionConfiguration.Instance();


        private void UpdateUI(Action action)
        {
            this.Dispatcher.BeginInvoke(action);
        }
        private void DoGetCarInfo(ConnectionConfiguration conn, string appCode)
        {
            bool flag = string.IsNullOrEmpty(appCode);
            if (!flag)
            {
                Task.Run(() =>
                {
                    Action action = null;
                    if (action == null)
                    {
                        action = () =>
                        {
                            this.txtInfo.Text = "Đang lấy thông tin...";
                        };
                    }

                    UpdateUI(action);
                    OracleParameter oraCarCodePar = new OracleParameter("CARCODE", appCode);
                    using (OracleConnection appContext = new OracleConnection(conn.AppConnectionString))
                    {
                        List<string> lstContent = new List<string>();
                        List<string> assignUser = appContext.SqlQuery<string>("SELECT SU.USER_ID\r\n  " +
                            "FROM CAR_STAGE cs, CAR, SYS_USER SU\r\n " +
                            "WHERE     cs.APPLICATION_ID = CAR.ID\r\n       " +
                            "AND CAR.APPLICATION_CODE = :CARCODE\r\n       " +
                            "AND CAR.AMND_STATE = 'F'\r\n       AND cs.ACTION IS NULL\r\n       " +
                            "AND cs.ASSIGNEE_ID = SU.ID(+)",
                            oraCarCodePar).ToList();
                        bool flag2 = assignUser.Count((string e) => !string.IsNullOrEmpty(e)) != 0;
                        if (flag2)
                        {
                            this.UpdateUI(delegate
                            {
                                this.txtInfo.Text = "Hồ sơ đã được assign cho user " + string.Join(",", (from e in assignUser
                                                                                                         where !string.IsNullOrEmpty(e)
                                                                                                         select e).ToArray<string>());
                            });
                        }
                        else
                        {
                            using (OracleConnection bpmContext = new OracleConnection(conn.BPMConnectionString))
                            {
                                string sql = @"SELECT U.ORGANIZATIONAL_UNIT_NAME OU, W.*
  FROM WFTASK W, BPM_ORGANIZATIONAL_UNIT U
 WHERE     (   PROTECTEDTEXTATTRIBUTE11 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE12 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE13 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE14 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE15 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE16 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE17 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE18 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE19 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE20 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE1 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE2 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE3 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE4 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE5 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE6 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE7 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE8 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE9 LIKE :CARCODE
            OR PROTECTEDTEXTATTRIBUTE10 LIKE :CARCODE)
       AND W.ORGANIZATIONALUNITID = U.ORGANIZATIONAL_UNIT_ID(+)";
                                var tmp = bpmContext.SqlQuery<WFTASK>(sql, new OracleParameter[] { oraCarCodePar }).ToList();
                                List<WFTASK> listTask = (from x in tmp
                                                         where x.STATE == "OPEN" || x.STATE == "ASSIGNED"
                                                         select x).ToList();
                                foreach (WFTASK task in listTask)
                                {
                                    if (lstContent.Any())
                                    {
                                        lstContent.Add("\n");
                                    }
                                    if (string.IsNullOrEmpty(task.ASSIGNEES))
                                    {
                                        lstContent.Add(string.Concat(new string[]
                                        {
                                        "Bước \"",
                                        task.TITLE,
                                        "\", TaskId = ",
                                        task.TASKID,
                                        ", Users = Không có thông tin user nhận việc",
                                        }));
                                        continue;
                                    }
                                    if (task.ASSIGNEES.Contains(",user"))
                                    {

                                        lstContent.Add(string.Concat(new string[]
                                        {
                                        "Bước \"",
                                        task.TITLE,
                                        "\", TaskId = ",
                                        task.TASKID,
                                        ", Users = ",
                                        task.ASSIGNEES.Replace(",user","")
                                        }));
                                        continue;
                                    }
                                    else
                                    {
                                        List<string> lstUser = appContext.SqlQuery<string>("SELECT SU.USER_ID\r\n  " +
                                        "FROM SYS_ROLE  SR,\r\n       SYS_USER_ROLE      SUR,\r\n       DEPARTMENT         D,\r\n       SYS_USER           SU\r\n " +
                                        "WHERE     UPPER (SR.EXT_REF_NO_1) = UPPER (:role)\r\n       AND SR.AMND_STATE = 'F'\r\n       " +
                                        "AND D.DEPT_ID = :ou\r\n       AND SUR.AMND_STATE = 'F'\r\n       " +
                                        "AND D.AMND_STATE = 'F'\r\n       AND SUR.ROLE_ID = SR.ID\r\n       " +
                                        "AND D.ID = SUR.DEPT_ID\r\n       AND SUR.USER_ID = SU.ID\r\n      " +
                                        " AND SU.AMND_STATE = 'F'", new OracleParameter[]
                                    {
                                        new OracleParameter("role", task.ASSIGNEES),
                                        new OracleParameter("ou", task.OU)
                                    }).ToList();
                                        lstContent.Add(string.Concat(new string[]
                                        {
                                        "Bước \"",
                                        task.TITLE,
                                        "\", TaskId = ",
                                        task.TASKID,
                                        ", Users = ",
                                        string.Join(", ", lstUser.ToArray())
                                        }));
                                    }

                                }
                            }
                            this.UpdateUI(delegate
                            {
                                this.txtInfo.Text = string.Join("\r\n", lstContent.ToArray());
                            });
                        }
                    }
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string appCode = this.txtAppCode.Text;
            ConnectionConfiguration conn = this.connectionConfiguration.ConnectionConfigurations[this.cbEnv.SelectedIndex];
            this.DoGetCarInfo(conn, appCode);
        }
    }
}

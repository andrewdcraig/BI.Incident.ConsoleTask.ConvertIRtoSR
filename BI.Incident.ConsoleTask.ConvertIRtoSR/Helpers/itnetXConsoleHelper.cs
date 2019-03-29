using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.Common;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters;
using System;
using System.Collections.Generic;

namespace BI.WorkItem.ConsoleTask.ConvertWI.Helpers
{
    public class itnetXConsoleHelper
    {
        private static object lockObject;

        private static itnetXConsoleHelper instance;
        public static itnetXConsoleHelper Instance
        {
            get
            {
                if (itnetXConsoleHelper.instance == null)
                {
                    lock (lockObject)
                    {
                        if (itnetXConsoleHelper.instance == null)
                        {
                            itnetXConsoleHelper.instance = new itnetXConsoleHelper();
                        }
                    }
                }


                return itnetXConsoleHelper.instance;
            }
        }

        private DataAccessQuery dataAccessQuery;


        static itnetXConsoleHelper()
        {
            lockObject = new object();
        }


        private itnetXConsoleHelper()
        {
            this.dataAccessQuery = new DataAccessQuery();
        }

        public IList<IDataItem> GetAllInstances(Guid managementPackClassId, IList<string> propertiesToFetch)
        {

            Dictionary<string, object> parameterList = new Dictionary<string, object>();
            parameterList.Add("ManagementPackClassId", managementPackClassId);
            if ((propertiesToFetch != null) && (propertiesToFetch.Count > 0))
            {
                parameterList.Add("PropertyList", propertiesToFetch);
            }
            IList<IDataItem> list = this.dataAccessQuery.QueryAdapter(parameterList, null, typeof(EnterpriseManagementObjectAdapter), EnterpriseManagementObjectAdapter.AdapterName);

            return list;
        }

        public IList<IDataItem> GetAllProjectionInstances(Guid typeProjectionId, IList<string> propertiesToFetch)
        {
            Dictionary<string, object> parameterList = new Dictionary<string, object>();
            parameterList.Add("TypeProjectionId", typeProjectionId);

            if ((propertiesToFetch != null) && (propertiesToFetch.Count > 0))
            {
                parameterList.Add("PropertyList", propertiesToFetch);
            }


            IList<IDataItem> list = this.dataAccessQuery.QueryAdapter(parameterList, null, typeof(EnterpriseManagementObjectProjectionAdapter), EnterpriseManagementObjectProjectionAdapter.AdapterName);

            return list;
        }

        public IList<IDataItem> GetObjectTemplate(string templateName)
        {


            Dictionary<string, object> parameterList = new Dictionary<string, object>();
            parameterList.Add("TemplateCriteria", string.Format("Name = '{0}'", templateName));
            IList<IDataItem> list = this.dataAccessQuery.QueryAdapter(parameterList, null, typeof(ManagementPackObjectTemplateAdapter), ManagementPackObjectTemplateAdapter.AdapterName);
            if ((list != null) && (list.Count != 0))
            {
                return list;
            }
            return null;

        }

        private static void PrepareNewActivity(IDataItem activity, bool IsTemplateMode)
        {
            if ((activity != null) && ((bool)activity["$IsNew$"]))
            {
                if (!IsTemplateMode)
                {
                    activity["ActivityCreatedBy"] = ConsoleContextHelper.Instance.CurrentUser;
                }
                string activityPrefix = ConsoleContextHelper.Instance.GetActivityPrefix(activity);
                string str2 = activity["Id"] as string;
                if (string.IsNullOrEmpty(str2) || !str2.Contains(activityPrefix))
                {
                    activity["Id"] = activityPrefix + activity["Id"];
                }
            }
        }

        public static void InitializeChildActivities(IDataItem parentWorkItem, bool IsTemplateMode)
        {
            if ((parentWorkItem != null) && ((bool)parentWorkItem["$IsNew$"]))
            {
                IList<IDataItem> list = parentWorkItem["Activity"] as IList<IDataItem>;
                foreach (IDataItem item in list)
                {
                    itnetXConsoleHelper.PrepareNewActivity(item, IsTemplateMode);
                    itnetXConsoleHelper.InitializeChildActivities(item, IsTemplateMode);
                }
            }
        }


    }
}

using Microsoft.EnterpriseManagement.ConsoleFramework;
using Microsoft.EnterpriseManagement.GenericForm;
using Microsoft.EnterpriseManagement.ServiceManager.Application.Common;
using Microsoft.EnterpriseManagement.ServiceManager.Applications.ServiceRequest.Forms;
using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.FormsInfra;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters;
using BI.WorkItem.ConsoleTask.ConvertWI.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace BI.WorkItem.ConsoleTask.ConvertWI.Tasks
{
    public class ConvertIRSRTaskHandler : ConsoleCommand
    {
        private IDataItem selectedTemplate = null;
        Window parentForm = null;
        public static object lockObject = new object();
        IDataItem emopSourceObject = null;
        object state = new object();

        public override void ExecuteCommand(IList<NavigationModelNodeBase> nodes, NavigationModelNodeTask task, ICollection<string> parameters)
        {
            if ((nodes != null) && (nodes.Count > 0))
            {
                emopSourceObject = FormUtilities.Instance.GetFormDataContext(nodes[0]);
                bool isForm = FormUtilities.Instance.IsNodeWithinForm(nodes[0]);
                // exit if running from view
                if (!isForm)
                    return;

                // find root control
                FormView formView = NavigationModel.FindView(null, nodes[0].Location, FindViewCriteria.ViewIsAssociatedToNode) as FormView;
                // get parent form
                parentForm = Window.GetWindow(formView);

                IDataItem classDataItem = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.GetClassType(Constants.Class_System_WorkItem_ServiceRequest);
                NavigationModelNodeTask createTask = null;

                if (parameters.Contains("TemplateId"))
                {
                    Guid templid = new Guid(parameters.Last());
                    selectedTemplate = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.GetObjectTemplate(templid);
                    createTask = NavigationTasksHelper.CreateNewInstanceLink(classDataItem, selectedTemplate);
                }
                else
                {
                    createTask = NavigationTasksHelper.CreateNewInstanceLink(classDataItem);
                }
               
                NavigationModelNodeBase nodeOut = null;
                //IDataItem projectionInstance = null;
                lock (ConvertIRSRTaskHandler.lockObject)
                {
                    List<FormViewOperation> operList = new List<FormViewOperation>();
                    IAsyncResult result;

                    result = GenericCommon.MonitorCreatedForm(nodes[0], createTask, out nodeOut, operList);

                    if ((result != null))
                    {
                        IDataItem projectionInstance = FormUtilities.Instance.GetFormDataContext(nodes[0]);
                        CallbackData state = new CallbackData(result, nodeOut, nodes, new List<IDataItem> { projectionInstance });
                        WaitOrTimerCallback callBack = new WaitOrTimerCallback(this.ExecuteCompletedCallback);
                        ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, callBack, state, -1, true);
                        Thread.Sleep(10);
                        //MessageBox.Show("Fill in Request form.");

                        while ((result != null) && !result.IsCompleted)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }

            }
        }

        protected virtual void ExecuteCompletedCallback(object state, bool timeout)
        {
            CallbackData data = state as CallbackData;
            NavigationModel.EndExecute(data.Result);
            IDataItem emopNewObject = FormUtilities.Instance.GetFormDataContext(data.CreatedNode);

            // find root control
            FormView formView = NavigationModel.FindView(null, data.CreatedNode.Location, FindViewCriteria.ViewIsAssociatedToNode) as FormView;
            // get parent form

            formView.Dispatcher.Invoke((Action)(() =>
            {
                if (formView.Form is System.Windows.Controls.TabControl)
                {
                    object firstTabItem = (formView.Form as System.Windows.Controls.TabControl).Items[0];
                    if (firstTabItem is TabItem)
                    {
                        ((firstTabItem as TabItem).Content as System.Windows.Controls.UserControl).AddHandler(FormEvents.SubmittedEvent, new EventHandler<FormCommandExecutedEventArgs>(this.OnNewFormSubmittedEventHandler), true);
                    }
                }
            }), null);

            //IDataItem projectionInstance = data.AddedItems[0];
            // properties and components to copy
            emopNewObject["Title"] = emopSourceObject["Title"];
            emopNewObject["Description"] = emopSourceObject["Description"];
            emopNewObject["CreatedDate"] = emopSourceObject["CreatedDate"];

            //emopNewObject["Source"] = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.GetEnumeration(Constants.Enum_IncidentSourceEnum_Console);


            if (emopSourceObject != null)
            {
                if (emopNewObject.HasProperty("AffectedUser"))
                {
                    emopNewObject["AffectedUser"] = emopSourceObject["AffectedUser"];
                }

                //if (emopNewObject.HasProperty("FileAttachments"))
                //{
                //    emopNewObject["FileAttachments"] = ConsoleTaskHelper.CopyFileAttachments((IList<IDataItem>)emopSourceObject["FileAttachments"]);
                //}

                //if (emopNewObject.HasProperty("RelatedWorkItems"))
                //{
                //    emopNewObject["RelatedWorkItems"] = emopSourceObject;
                //}

                // create action log, skipped for this release
                //if (newItem.HasProperty("ActionLogs"))
                //{

                //    IDataItem dataItem = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.CreateNewInstanceBindableItem(Constants.Class_System_WorkItem_TroubleTicket_ActionLog);
                //    dataItem["EnteredDate"] = DateTime.Now.ToUniversalTime();
                //    dataItem["Id"] = Guid.NewGuid().ToString();
                //    dataItem["ActionType"] = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.GetEnumeration(Constants.Enum_System_WorkItem_ActionLogEnum_RecordOpened);
                //    dataItem["EnteredBy"] = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.CurrentUser["DisplayName"].ToString();
                //    dataItem["Title"] = "Aus E-Mail Request erstellt";
                //    dataItem["Description"] = string.Format(CultureInfo.CurrentUICulture, "Eintrag aus E-Mail Request [{0}] erstellt", new object[] { projectionInstance["Id"] });
                //    newItem["ActionLogs"] = dataItem;
                //}
            }

           

            itnetXConsoleHelper.InitializeChildActivities(emopNewObject, false);

            base.RequestViewRefresh();

        }

        private void OnNewFormSubmittedEventHandler(object sender, FormCommandExecutedEventArgs e)
        {
            // Update Status of Service Request
            emopSourceObject["Status"] = Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.GetEnumeration(Constants.Enum_System_WorkItem_ServiceRequest_StatusEnum_Closed);
            emopSourceObject["ActualEndDate"] = DateTime.Now.ToUniversalTime();
            Microsoft.EnterpriseManagement.UI.Extensions.Shared.ConsoleContextHelper.Instance.UpdateInstance(emopSourceObject);

            parentForm.Dispatcher.Invoke((Action)(() =>
            {
                parentForm.Close();
            }), null);

            base.RequestViewRefresh();
        }
    }
}

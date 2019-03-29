using Microsoft.EnterpriseManagement.UI.DataModel;
using Microsoft.EnterpriseManagement.UI.Extensions.Shared;
using Microsoft.EnterpriseManagement.UI.SdkDataAccess.DataAdapters;
using Microsoft.EnterpriseManagement.UI.WpfControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BI.WorkItem.ConsoleTask.ConvertWI.Helpers
{
    public static class ConsoleTaskHelper
    {
        static public ObservableCollection<IDataItem> CopyFileAttachments(IList<IDataItem> SourceFileAttachments)
        {
            ObservableCollection<IDataItem> wiAttachments = new ObservableCollection<IDataItem>();

            foreach (IDataItem curAttachment in SourceFileAttachments)
            {
                IDataItem item = ConsoleContextHelper.Instance.CreateProjectionInstance(Constants.TP_System_FileAttachmentProjection, Constants.Class_System_FileAttachment);
                item["FileAttachmentAddedBy"] = curAttachment["FileAttachmentAddedBy"];
                item["Extension"] = curAttachment["Extension"];
                item["Size"] = curAttachment["Size"];
                item["AddedDate"] = curAttachment["AddedDate"]; ;
                item["Id"] = Guid.NewGuid().ToString();
                item["Content"] = curAttachment["Content"];
                item["DisplayName"] = curAttachment["DisplayName"];
                wiAttachments.Add(item);
            }

            return wiAttachments;

        }

        static public IDataItem GetRootWI(IDataItem activityItem)
        {
            IDataItem rootItem = activityItem;

            do
            {
                ReadOnlyCollection<IDataItem> parentWIs = ConsoleContextHelper.Instance.GetRelationships((Guid)rootItem["$Id$"], Constants.Rel_System_WorkItemContainsActivity, false);
                if (parentWIs != null && parentWIs.Count > 0)
                {
                    rootItem = (IDataItem)parentWIs[0]["Source"];
                }
                else
                    rootItem = null;

            } while (rootItem != null && !ManagementPackClassDataType.InFamilyOf((IDataItem)rootItem["$Class$"], Constants.Class_System_WorkItem_ServiceRequest));

            return rootItem;
        }

        static public IDataItem ShowTemplatePicker(Guid classId)
        {
            TemplatePickerDialog templatePickerDialog = new TemplatePickerDialog();
            templatePickerDialog.SelectedTypeId = classId;
            templatePickerDialog.TemplateTargetType = TemplatePicker.TemplateType.DerivedAndProjectionTypes;
            templatePickerDialog.AllowCategorySelection = false;
            bool? result = templatePickerDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return templatePickerDialog.PickedTemplate;
            }

            return null;
        }
    }
}

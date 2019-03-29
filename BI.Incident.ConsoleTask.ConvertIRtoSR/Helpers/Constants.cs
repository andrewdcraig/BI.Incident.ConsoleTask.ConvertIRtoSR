using System;

namespace BI.WorkItem.ConsoleTask.ConvertWI.Helpers
{
    internal class Constants
    {
        public static readonly Guid MP_BI_WorkItem_ConsoleTask_ConvertWI = new Guid("e7fb246d-ac2b-1d50-725d-1ab019de1f50");

        public static readonly Guid Class_System_WorkItem_ServiceRequest = new Guid("04b69835-6343-4de2-4b19-6be08c612989");
        public static readonly Guid Class_System_FileAttachment = new Guid("68a35b6d-ca3d-8d90-f93d-248ceff935c0");
        public static readonly Guid Class_System_WorkItem_TroubleTicket_ActionLog = new Guid("DBB6A632-0A7E-CEF8-1FC9-405D5CD4D911");
        public static readonly Guid Class_System_WorkItem_Incident = new Guid("a604b942-4c7b-2fb2-28dc-61dc6f465c68");


        // from ServiceManager.IncidentManagement.Library
        public static readonly Guid TP_System_FileAttachmentProjection = new Guid("42561c7a-5d16-a5fe-64f9-ffa5bbc6cdb5");
        public static readonly Guid Rel_System_WorkItemContainsActivity = new Guid("2da498be-0485-b2b2-d520-6ebd1698e61b");

        public static readonly Guid Enum_System_WorkItem_ActionLogEnum_RecordOpened = new Guid("57c84711-ab28-291a-793b-60d6532a35e3");
        public static readonly Guid Enum_IncidentSourceEnum_Console = new Guid("76480d55-a19d-7cef-4446-0f1ccaef11ce");
        public static readonly Guid Enum_System_WorkItem_ServiceRequest_StatusEnum_Closed = new Guid("c7b65747-f99e-c108-1e17-3c1062138fc4");

        public static LocalizationHelper LocalizationHelper = new LocalizationHelper("SC.WorkItem.ConsoleTasks.Forms.Strings", Constants.MP_BI_WorkItem_ConsoleTask_ConvertWI);
    }
}

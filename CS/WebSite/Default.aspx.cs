﻿using System;
using System.Web.UI.WebControls;
using DevExpress.XtraScheduler;
using DevExpress.Web.ASPxScheduler;

public partial class Default : System.Web.UI.Page
{
    ASPxSchedulerStorage Storage { get { return ASPxScheduler1.Storage; } }

    protected void Page_Load(object sender, EventArgs e) {
        SetupMappings();
        ResourceFiller.FillResources(this.ASPxScheduler1.Storage, 3);

        ASPxScheduler1.AppointmentDataSource = appointmentDataSource;
        ASPxScheduler1.DataBind();

        ASPxScheduler1.GroupType = SchedulerGroupType.Resource;
    }

    protected void ASPxScheduler1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
        if (e.Menu.MenuId == SchedulerMenuItemId.AppointmentMenu) {
            e.Menu.ClientSideEvents.ItemClick = string.Format("function(s, e) {{ DefaultAppointmentMenuHandler({0}, s, e); }}",
                ASPxScheduler1.ClientInstanceName);
        }
    }

    void SetupMappings() {
        ASPxAppointmentMappingInfo mappings = Storage.Appointments.Mappings;
        Storage.BeginUpdate();
        try {
            mappings.AppointmentId = "Id";
            mappings.Start = "StartTime";
            mappings.End = "EndTime";
            mappings.Subject = "Subject";
            mappings.AllDay = "AllDay";
            mappings.Description = "Description";
            mappings.Label = "Label";
            mappings.Location = "Location";
            mappings.RecurrenceInfo = "RecurrenceInfo";
            mappings.ReminderInfo = "ReminderInfo";
            mappings.ResourceId = "OwnerId";
            mappings.Status = "Status";
            mappings.Type = "EventType";
        }
        finally {
            Storage.EndUpdate();
        }
    }

    //Initilazing ObjectDataSource
    protected void appointmentsDataSource_ObjectCreated(object sender, ObjectDataSourceEventArgs e) {
        e.ObjectInstance = new CustomEventDataSource(GetCustomEvents());
    }

    CustomEventList GetCustomEvents() {
        CustomEventList events = Session["ListBoundModeObjects"] as CustomEventList;
        if (events == null) {
            events = GenerateCustomEventList();
            Session["ListBoundModeObjects"] = events;
        }
        return events;
    }

    // User generated appointment id    
    protected void ASPxScheduler1_AppointmentInserting(object sender, PersistentObjectCancelEventArgs e) {
        SetAppointmentId(sender, e);
    }

    void SetAppointmentId(object sender, PersistentObjectCancelEventArgs e) {
        ASPxSchedulerStorage storage = (ASPxSchedulerStorage)sender;
        Appointment apt = (Appointment)e.Object;
        storage.SetAppointmentId(apt, apt.GetHashCode());
    }

    #region Random events generation
    CustomEventList GenerateCustomEventList() {
        CustomEventList eventList = new CustomEventList();
        int count = Storage.Resources.Count;
        for (int i = 0; i < count; i++) {
            Resource resource = Storage.Resources[i];
            string subjPrefix = resource.Caption + "'s ";

            eventList.Add(CreateEvent(resource.Id, subjPrefix + "meeting", 2, 5));
            eventList.Add(CreateEvent(resource.Id, subjPrefix + "travel", 3, 6));
            eventList.Add(CreateEvent(resource.Id, subjPrefix + "phone call", 0, 10));
        }
        return eventList;
    }
    CustomEvent CreateEvent(object resourceId, string subject, int status, int label) {
        CustomEvent customEvent = new CustomEvent();
        customEvent.Subject = subject;
        customEvent.OwnerId = resourceId;
        Random rnd = rndInstance;
        int rangeInHours = 48;
        customEvent.StartTime = DateTime.Today + TimeSpan.FromHours(rnd.Next(0, rangeInHours));
        customEvent.EndTime = customEvent.StartTime + TimeSpan.FromHours(rnd.Next(0, rangeInHours / 8));
        customEvent.Status = status;
        customEvent.Label = label;
        customEvent.Id = "ev" + customEvent.GetHashCode();
        return customEvent;
    }
    static Random rndInstance = new Random();
    #endregion
}

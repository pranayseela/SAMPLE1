using System;
using System.Collections.Generic;
using System.Linq;
using TEER.Helpers;
using TEER.Model;

namespace TEER.ViewModel
{
    public class TimeEntryViewModel : BaseUIViewModel
    {
        public bool IsValid
        {
            get
            {
                return ErrorCtl.Count == 0;
            }
        }

        public Dictionary<string, string> ErrorCtl { get; private set; }

        /// <summary>
        /// Guid to identify the entry
        /// </summary>
        public string GuidId { get; set; }

        public decimal HoursOfServiceId { get; set; }


        public int EntryTypeId { get; set; }


        public string BeginLabelText { get; set; }

        public string EndLabelText { get; set; }


        public int? BeginLocationId { get; set; }

        public string BeginLocationName { get; set; }

        public string BeginDate { get; set; }

        public string BeginTime { get; set; }


        public int? EndLocationId { get; set; }

        public string EndLocationName { get; set; }

        public string EndDate { get; set; }

        public string EndTime { get; set; }
        

        public string Train1 { get; set; }

        public string Train2 { get; set; }


        public string Train1Old { get; set; }

        public string Train2Old { get; set; }


        /// <summary>
        /// Gets the timespan between BeginTime and EndTime
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                DateTime? beginTime = Helper.ConvertToDateTime(BeginDate, BeginTime);
                DateTime? endTime = Helper.ConvertToDateTime(EndDate, EndTime);

                if (beginTime == null || endTime == null)
                {
                    return TimeSpan.Zero;
                }

                if (endTime < beginTime)
                {
                    return TimeSpan.Zero;
                }

                return endTime.Value - beginTime.Value;
            }
        }


        public int? TransitTypeId { get; set; }

        public string TransitTypeName { get; set; }


        public int BeforeAfterFlag { get; set; }

        public int DisplayOrder { get; set; }


        public bool PreventDeleteEntry { get; set; }

        //public bool TransitTypeRequired
        //{
        //    get
        //    {
        //        // transit mode is always required for "deadhead"
        //        return (TransitTypeId == null && (EntryTypeId == (int)TimeEntryTypeEnum.DeadheadFrom || EntryTypeId == (int)TimeEntryTypeEnum.DeadheadTo));
        //    }
        //}


        public TimeEntryViewModel()
        {
            GuidId = Guid.NewGuid().ToString();
            ErrorCtl = new Dictionary<string, string>();

            BeginLabelText = "BEGIN";
            EndLabelText = "END";
        }

        //public TimeEntryViewModel(TimeEntry te)
        //    : this()
        //{
        //    HoursOfServiceId = te.HoursOfServiceId;
        //    EntryTypeId = te.EntryTypeId;

        //    BeginLocationId = te.BeginLocationId;
        //    BeginLocationName = te.BeginLocationName;

        //    // ignore the database assigned 01/01/1900 date
        //    if (te.BeginTime != null && te.BeginTime.Value.Year > 1900)
        //    {
        //        BeginDate = te.BeginTime.Value.ToEffDateFormat();
        //        BeginTime = te.BeginTime.Value.ToEffTimeFormat();
        //    }

        //    EndLocationId = te.EndLocationId;
        //    EndLocationName = te.EndLocationName;

        //    // ignore the database assigned 01/01/1900 date
        //    if (te.EndTime != null && te.EndTime.Value.Year > 1900)
        //    {
        //        EndDate = te.EndTime.Value.ToEffDateFormat();
        //        EndTime = te.EndTime.Value.ToEffTimeFormat();
        //    }

        //    Train1 = te.Train1;
        //    Train2 = te.Train2;

        //    TransitTypeId = te.TransitTypeId;
        //    TransitTypeName = te.TransitTypeName;

        //    BeforeAfterFlag = te.BeforeAfterFlag;
        //    DisplayOrder = te.DisplayOrder;
        //}


        //public void Validate(bool validateTimeFieldsOnly = false)
        //{
        //    ErrorCtl.Clear();

        //    if (validateTimeFieldsOnly)
        //    {
        //        //ErrorMap.Clear();
        //        ValidateTime();
        //    }
        //    else
        //    {
        //        ValidateTime();
        //        ValidateNonTimeFields();
        //    }
        //}


        /// <summary>
        /// Checks if the end time of the prior entry is less than the begin time of this time entry
        /// </summary>
        public void ValidateBeginTimeIsGreaterThan(DateTime priorEntryEndTime, string errorMessage)
        {
            DateTime? beginTime = Helper.ConvertToDateTime(BeginDate, BeginTime);

            if (beginTime != null && beginTime.Value < priorEntryEndTime)
            {
                if (!ErrorCtl.ContainsKey("txtBeginDate"))
                {
                    ErrorCtl["txtBeginDate"] = errorMessage;
                }
            }
        }

        /// <summary>
        /// Checks if the end time of the prior entry is equal to the begin time of this time entry
        /// </summary>
        public void ValidateBeginTimeIsEqualTo(DateTime priorEntryEndTime, string errorMessage)
        {
            DateTime? beginTime = Helper.ConvertToDateTime(BeginDate, BeginTime);

            if (beginTime != null && beginTime.Value == priorEntryEndTime)
            {
                if (!ErrorCtl.ContainsKey("txtBeginDate"))
                {
                    ErrorCtl["txtBeginDate"] = errorMessage;
                }
            }
        }


        /// <summary>
        /// Checks if the end time of this time entry is less than the begin time of nextEntry
        /// </summary>
        public void ValidateEndTimeIsLessThan(DateTime nextEntryBeginTime, string errorMessage)
        {
            DateTime? endTime = Helper.ConvertToDateTime(EndDate, EndTime);

            if (endTime != null && endTime.Value > nextEntryBeginTime)
            {
                if (!ErrorCtl.ContainsKey("txtEndDate"))
                {
                    ErrorCtl["txtEndDate"] = errorMessage;
                }
            }
        }

        /// <summary>
        /// Checks if the end time of this time entry is equal to the begin time of nextEntry
        /// </summary>
        public void ValidateEndTimeIsEqualTo(DateTime nextEntryBeginTime, string errorMessage)
        {
            DateTime? endTime = Helper.ConvertToDateTime(EndDate, EndTime);

            if (endTime != null && endTime.Value == nextEntryBeginTime)
            {
                if (!ErrorCtl.ContainsKey("txtEndDate"))
                {
                    ErrorCtl["txtEndDate"] = errorMessage;
                }
            }
        }


        //public TimeEntry GetTimeEntry()
        //{
        //    TimeEntry te = new TimeEntry();

        //    te.EntryTypeId = EntryTypeId;

        //    te.BeginLocationId = BeginLocationId;
        //    te.BeginLocationName = BeginLocationName;
        //    te.BeginTime = Helper.ConvertToDateTime(BeginDate, BeginTime);

        //    te.EndLocationId = EndLocationId;
        //    te.EndLocationName = EndLocationName;
        //    te.EndTime = Helper.ConvertToDateTime(EndDate, EndTime);

        //    te.Train1 = Train1;
        //    te.Train2 = Train2;

        //    te.TransitTypeId = TransitTypeId;
        //    te.TransitTypeName = TransitTypeName;

        //    te.BeforeAfterFlag = BeforeAfterFlag;
        //    te.DisplayOrder = DisplayOrder;

        //    return te;
        //}


        //internal void ValidateNonTimeFields(List<string> trainNames = null)
        //{
        //    if (BeginLocationId == null)
        //    {
        //        if (string.IsNullOrEmpty(BeginLocationName))
        //        {
        //            ErrorCtl["txtBeginLocation"] = Msg.ERROR_ENTER_LOCATION;
        //        }
        //        else
        //        {
        //            ErrorCtl["txtBeginLocation"] = Msg.ERROR_ENTER_VALID_LOCATION;
        //        }
        //    }

        //    if (EndLocationId == null)
        //    {
        //        if (string.IsNullOrEmpty(EndLocationName))
        //        {
        //            ErrorCtl["txtEndLocation"] = Msg.ERROR_ENTER_LOCATION;
        //        }
        //        else
        //        {
        //            ErrorCtl["txtEndLocation"] = Msg.ERROR_ENTER_VALID_LOCATION;
        //        }
        //    }

        //    if ((BeginLocationId.HasValue && EndLocationId.HasValue) && (BeginLocationId.Value == EndLocationId.Value))
        //    {
        //        ErrorCtl["txtBeginLocation"] = Msg.ERROR_BEGIN_LOCATION_SAME_AS_END_LOCATION;
        //    }

        //    // ** this validation is required only for "on duty" & "interim release" **
        //    // validate train name textbox. if values are entered in the textbox they should also appear in the train names list
        //    if (EntryTypeId == (int)TimeEntryTypeEnum.OnDuty || EntryTypeId == (int)TimeEntryTypeEnum.InterimRelease)
        //    {
        //        if (!string.IsNullOrEmpty(Train1) && !trainNames.Exists(t => t.Equals(Train1)))
        //        {
        //            ErrorCtl["txtTrain1"] = Msg.ERROR_ADD_TRAIN_TO_LIST;
        //        }

        //        if (!string.IsNullOrEmpty(Train2) && !trainNames.Exists(t => t.Equals(Train2)))
        //        {
        //            ErrorCtl["txtTrain2"] = Msg.ERROR_ADD_TRAIN_TO_LIST;
        //        }
        //    }

        //    // if (TransitTypeId == null && (EntryTypeId == (int)TimeEntryTypeEnum.DeadheadFrom || EntryTypeId == (int)TimeEntryTypeEnum.DeadheadTo))
        //    if (TransitTypeRequired)
        //    {
        //        ErrorCtl["ddlTransitType"] = Msg.ERROR_SELECT_TRANSIT_MODE;
        //    }

        //    // train name is required for "deadhead" if transit type is Train
        //    //if (TransitTypeId == (int)TransitTypeEnum.Train
        //    //    && (entryType == TimeEntryTypeEnum.DeadheadTo || entryType == TimeEntryTypeEnum.DeadheadFrom)
        //    //    && string.IsNullOrEmpty(Train1))
        //    //{
        //    //    ErrorMap["txtTrain1"] = Msg.ERROR_ENTER_TRAIN_NAME;
        //    //}
        //}

        //void ValidateTime()
        //{
        //    TimeEntryTypeEnum entryType = (TimeEntryTypeEnum)Enum.ToObject(typeof(TimeEntryTypeEnum), EntryTypeId);

        //    // onduty & interim release are validated in the parent viewmodel
        //    // ot move & seniority move don't require date & time
        //    if (entryType == TimeEntryTypeEnum.OnDuty || entryType == TimeEntryTypeEnum.InterimRelease
        //        || entryType == TimeEntryTypeEnum.OTMove || entryType == TimeEntryTypeEnum.SeniorityMove)
        //    {
        //        // ignore
        //        return;
        //    }

        //    DateTime? beginTime = Helper.ConvertToDateTime(BeginDate, BeginTime);
        //    DateTime? endTime = Helper.ConvertToDateTime(EndDate, EndTime);

        //    if (beginTime == null)
        //    {
        //        ErrorCtl["txtBeginDate"] = Msg.ERROR_VALID_DATE_AND_TIME;
        //    }

        //    if (endTime == null)
        //    {
        //        ErrorCtl["txtEndDate"] = Msg.ERROR_VALID_DATE_AND_TIME;
        //    }

        //    //// for entries appearing in the "before" section validate begin time only. end time is optional
        //    //if (entryType == TimeEntryTypeEnum.DeadheadTo
        //    //    || entryType == TimeEntryTypeEnum.OTMove
        //    //    || entryType == TimeEntryTypeEnum.SeniorityMove)
        //    //{
        //    //    if (beginTime == null)
        //    //    {
        //    //        ErrorMap["txtBeginDate"] = Msg.ERROR_VALID_DATE_AND_TIME;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    // for entries appearing in the "after" section validate end time only. begin time is optional
        //    //    if (endTime == null)
        //    //    {
        //    //        ErrorMap["txtEndDate"] = Msg.ERROR_VALID_DATE_AND_TIME;
        //    //    }
        //    //}

        //    // BEGIN time must be less than END time
        //    if ((beginTime.HasValue && endTime.HasValue) && (endTime.Value < beginTime.Value))
        //    {
        //        ErrorCtl["txtBeginDate"] = Msg.ERROR_BEGIN_TIME_LATER_THAN_END_TIME;
        //    }
        //}
    }
}
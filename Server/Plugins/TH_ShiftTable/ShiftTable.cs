﻿// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Xml;
using System.Reflection;

using TH_Configuration;
using TH_Database;
using TH_GeneratedData;
using TH_Global;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_ShiftTable
{
    public class ShiftTable : IServerPlugin
    {

        #region "PlugIn"

        public string Name { get { return "TH_ShiftTable"; } }

        public void Initialize(Configuration config)
        {
            ShiftConfiguration sc = ShiftConfiguration.ReadXML(config.ConfigurationXML);

            if (sc != null)
            {
                config.CustomClasses.Add(sc);

                configuration = config;

                CreateShiftSegmentsTable(sc.shifts);
                CreateTable();

                rowInfos = GetExistingValues();
            }
        }


        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.id.ToLower() == "generatedeventitems")
                {                          
                    List<GeneratedData.GeneratedEventItem> genEventItems = (List<GeneratedData.GeneratedEventItem>)data.data02;

                    ProcessShifts(genEventItems);
                }
                else if (data.id.ToLower() == "mtconnect_current")
                {
                    Update_CurrentShiftInfo((TH_MTConnect.Streams.ReturnData)data.data02);
                }
            }
        }

        public void Update_CurrentShiftInfo(TH_MTConnect.Streams.ReturnData returnData)
        {
            currentData = returnData;

            // Update shift_current in "Variables" table
            CurrentShiftInfo shiftInfo = CurrentShiftInfo.Get(configuration, returnData.header.creationTime);
            if (UseDatabases)
            {
                if (shiftInfo != null)
                {
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_name", shiftInfo.name, returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_id", shiftInfo.id, returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_type", shiftInfo.type, returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_date", shiftInfo.date, returnData.header.creationTime, TablePrefix);

                    // Local
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_begintime", shiftInfo.shiftStart.ToString(), returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_endtime", shiftInfo.shiftEnd.ToString(), returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_currenttime", shiftInfo.currentTime.ToString(), returnData.header.creationTime, TablePrefix);

                    // UTC
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_begintime_utc", shiftInfo.shiftStartUTC.ToString(), returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_endtime_utc", shiftInfo.shiftEndUTC.ToString(), returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_currenttime_utc", shiftInfo.currentTimeUTC.ToString(), returnData.header.creationTime, TablePrefix);
                }
                else
                {
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_name", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_id", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_type", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_date", "", returnData.header.creationTime, TablePrefix);

                    // Local
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_begintime", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_endtime", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_currenttime", "", returnData.header.creationTime, TablePrefix);

                    // UTC
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_begintime_utc", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_endtime_utc", "", returnData.header.creationTime, TablePrefix);
                    TH_Database.Tables.Variables.Update(configuration.Databases_Server, "shift_currenttime_utc", "", returnData.header.creationTime, TablePrefix);
                }
            }

            var data = new EventData();
            data.id = "ShiftTable_CurrentShiftInfo";
            data.data01 = configuration;
            data.data02 = shiftInfo;
            if (SendData != null) SendData(data);
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing()
        {

        }

        //public TH_Plugins_Server.ConfigurationPage ConfigurationPage { get { return new ConfigurationPage.Page(); } }

        //public Type Config_Page { get { return typeof(ConfigurationPage.Page); } }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

        public bool UseDatabases { get; set; }

        public string TablePrefix { get; set; }

        #endregion

        #region "Properties"

        Configuration configuration { get; set; }

        #endregion

        #region "Methods"

        TH_MTConnect.Streams.ReturnData currentData;

        #region "Database"

        void CreateShiftSegmentsTable(List<Shift> shifts)
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("SHIFT", DataType.LargeText),
                new ColumnDefinition("SHIFT_ID", DataType.LargeText),
                new ColumnDefinition("SEGMENT_ID", DataType.LargeText),
                new ColumnDefinition("START", DataType.LargeText),
                new ColumnDefinition("END", DataType.LargeText),
                new ColumnDefinition("START_UTC", DataType.LargeText),
                new ColumnDefinition("END_UTC", DataType.LargeText),
                new ColumnDefinition("TYPE", DataType.LargeText)
            };

            var primaryKey = new string[] { "SHIFT_ID", "SEGMENT_ID" };

            Table.Replace(configuration.Databases_Server, TablePrefix + TableNames.ShiftSegments, columns.ToArray(), primaryKey);

            if (shifts != null)
            {
                List<string> insertColumns = new List<string>();
                insertColumns.Add("SHIFT");
                insertColumns.Add("SHIFT_ID");
                insertColumns.Add("SEGMENT_ID");
                insertColumns.Add("START");
                insertColumns.Add("END");
                insertColumns.Add("START_UTC");
                insertColumns.Add("END_UTC");
                insertColumns.Add("TYPE");

                List<List<object>> rowValues = new List<List<object>>();

                foreach (var shift in shifts)
                {
                    foreach (var segment in shift.segments)
                    {
                        List<object> values = new List<object>();
                        values.Add(shift.name);
                        values.Add(shift.id.ToString());
                        values.Add(segment.id.ToString());
                        values.Add(segment.beginTime.To24HourString());
                        values.Add(segment.endTime.To24HourString());
                        values.Add(segment.beginTime.ToUTC());
                        values.Add(segment.endTime.ToUTC());
                        values.Add(segment.type);
                        rowValues.Add(values);
                    }
                }

                Row.Insert(configuration.Databases_Server, TablePrefix + TableNames.ShiftSegments, insertColumns.ToArray(), rowValues, primaryKey, true);
            }
        }


        public string TableName = TableNames.Shifts;
        string[] primaryKey = { "ID" };

        List<string> ShiftTableColumns;

        List<string> GenEventColumns;

        void CreateTable()
        {
            if (configuration.DatabaseId != null) TableName = configuration.DatabaseId + "_" + TableNames.Shifts;
            else TableName = TableNames.Shifts;

            List<ColumnDefinition> columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("DATE", DataType.LargeText));
            columns.Add(new ColumnDefinition("SHIFT", DataType.LargeText));
            columns.Add(new ColumnDefinition("SEGMENTID", DataType.Long));
            columns.Add(new ColumnDefinition("START", DataType.LargeText));
            columns.Add(new ColumnDefinition("END", DataType.LargeText));
            columns.Add(new ColumnDefinition("START_UTC", DataType.LargeText));
            columns.Add(new ColumnDefinition("END_UTC", DataType.LargeText));
            columns.Add(new ColumnDefinition("TYPE", DataType.LargeText));
            columns.Add(new ColumnDefinition("TOTALTIME", DataType.Long));

            GenEventColumns = new List<string>();

            // Get ColumnDefinitions for each Generated Event
            var genEventColumnNames = GetGeneratedEventColumnNames(configuration);
            foreach (var columnName in genEventColumnNames)
            {
                GenEventColumns.Add(columnName);

                columns.Add(new ColumnDefinition(columnName, DataType.Long));
            }

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(configuration.Databases_Server, TableName, ColArray, primaryKey);  
        }

        void GetTableColumns()
        {
            ShiftTableColumns = Column.Get(configuration.Databases_Server, TableName);
        }

        static List<string> GetGeneratedEventColumnNames(Configuration config)
        {
            var result = new List<string>();

            ShiftConfiguration sc = ShiftConfiguration.Get(config);
            if (sc != null)
            {
                GeneratedData.GenDataConfiguration gdc = GeneratedData.GetConfiguration(config);
                if (gdc != null)
                {
                    foreach (GeneratedData.GeneratedEvents.Event genEvent in gdc.generatedEvents.events)
                    {
                        string columnName;

                        foreach (GeneratedData.GeneratedEvents.Value value in genEvent.Values)
                        {
                            columnName = Tools.FormatColumnName(genEvent, value);

                            result.Add(columnName);
                        }

                        // Add GenEvent.Default column
                        columnName = Tools.FormatColumnName(genEvent.Name, genEvent.Default.NumVal, genEvent.Default.Value);

                        result.Add(columnName);
                    }
                }
            }

            return result;
        }

        //void AddGeneratedEventColumns()
        //{
        //    ShiftConfiguration sc = ShiftConfiguration.Get(configuration);
        //    if (sc != null)
        //    {
        //        GeneratedData.GenDataConfiguration gdc = GeneratedData.GetConfiguration(configuration);
        //        if (gdc != null)
        //        {
        //            GenEventColumns = new List<string>();

        //            foreach (GeneratedData.GeneratedEvents.Event genEvent in gdc.generatedEvents.events)
        //            {
        //                string columnName;

        //                foreach (GeneratedData.GeneratedEvents.Value value in genEvent.Values)
        //                {
        //                    columnName = Tools.FormatColumnName(genEvent, value);

        //                    if (!ShiftTableColumns.Contains(columnName))
        //                    {
        //                        Column.Add(configuration.Databases_Server, TableName, new ColumnDefinition(columnName, DataType.Long));
        //                    }

        //                    GenEventColumns.Add(columnName);
        //                }

        //                // Add GenEvent.Default column
        //                columnName = Tools.FormatColumnName(genEvent.Name, genEvent.Default.NumVal, genEvent.Default.Value);

        //                if (!ShiftTableColumns.Contains(columnName))
        //                {
        //                    Column.Add(configuration.Databases_Server, TableName, new ColumnDefinition(columnName, DataType.Long));
        //                }

        //                GenEventColumns.Add(columnName);
        //            }
        //        }
        //    }
        //}

        List<ShiftRowInfo> GetExistingValues()
        {
            List<ShiftRowInfo> Result = new List<ShiftRowInfo>();

            if (UseDatabases)
            {
                DataTable dt = Table.Get(configuration.Databases_Server, TableName);
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        ShiftRowInfo sri = new ShiftRowInfo();

                        sri.id = row["id"].ToString();

                        DateTime date = DateTime.MinValue;
                        DateTime.TryParse(row["date"].ToString(), out date);
                        sri.date = new ShiftDate(date);

                        sri.shift = row["shift"].ToString();

                        int segmentid = -1;
                        int.TryParse(row["segmentid"].ToString(), out segmentid);
                        sri.segmentId = segmentid;

                        // get start time -----------------------------------------
                        date = DateTime.MinValue;
                        DateTime.TryParse(row["start"].ToString(), out date);
                        sri.start = new ShiftTime(date);
                        // --------------------------------------------------------

                        // get end time -------------------------------------------
                        date = DateTime.MinValue;
                        DateTime.TryParse(row["end"].ToString(), out date);
                        sri.end = new ShiftTime(date);
                        // --------------------------------------------------------

                        // get start time utc -------------------------------------
                        date = DateTime.MinValue;
                        DateTime.TryParse(row["start_utc"].ToString(), out date);
                        sri.start = new ShiftTime(date);
                        // --------------------------------------------------------

                        // get end time utc ---------------------------------------
                        date = DateTime.MinValue;
                        DateTime.TryParse(row["end_utc"].ToString(), out date);
                        sri.end = new ShiftTime(date);
                        // --------------------------------------------------------

                        sri.type = row["type"].ToString();

                        int totalTime = -1;
                        int.TryParse(row["totaltime"].ToString(), out totalTime);
                        sri.totalTime = totalTime;

                        // Get all of the rest of the columns as GenEventRowInfos
                        List<object> values = row.ItemArray.ToList();
                        if (values.Count > 8)
                        {
                            for (int x = 9; x <= values.Count - 1; x++)
                            {
                                GenEventRowInfo geri = new GenEventRowInfo();
                                geri.columnName = row.Table.Columns[x].ColumnName;

                                int seconds = -1;
                                int.TryParse(values[x].ToString(), out seconds);
                                geri.seconds = seconds;

                                sri.genEventRowInfos.Add(geri);
                            }
                        }

                        Result.Add(sri);

                    }
                }
            }
            
            return Result;

        }

        void UpdateShiftRows(List<ShiftRowInfo> infos)
        {
            List<List<object>> rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("id");
            columns.Add("date");
            columns.Add("shift");
            columns.Add("segmentid");
            columns.Add("start");
            columns.Add("end");
            columns.Add("start_utc");
            columns.Add("end_utc");
            columns.Add("type");

            foreach (string column in GenEventColumns) columns.Add(column);

            columns.Add("totaltime");

            foreach (ShiftRowInfo info in infos)
            {
                List<object> values = new List<object>();

                values.Add(info.id);
                values.Add(info.date.ToString());
                values.Add(info.shift);
                values.Add(info.segmentId);
                values.Add(info.start.ToString());
                values.Add(info.end.ToString());
                values.Add(info.start_utc.ToString());
                values.Add(info.end_utc.ToString());
                values.Add(info.type);

                foreach (string column in GenEventColumns)
                {
                    GenEventRowInfo geri = info.genEventRowInfos.Find(x => x.columnName.ToUpper() == column.ToUpper());
                    if (geri != null) values.Add(geri.seconds);
                    else values.Add(0);
                }

                values.Add(info.totalTime);

                rowValues.Add(values);
            }

            Row.Insert(configuration.Databases_Server, TableName, columns.ToArray(), rowValues, primaryKey, true);
        }

        #endregion

        #region "Processing"

        void ProcessShifts(List<GeneratedData.GeneratedEventItem> genEventItems)
        {
            List<GenEventShiftItem> genEventShiftItems = GenEventShiftItem.Get(configuration, genEventItems);

            // Send List of GenEventShiftItems to other Plugins
            SendGenEventShiftItems(genEventShiftItems);

            List<ShiftRowInfo> shiftRowInfos = ShiftRowInfo.Get(configuration, genEventShiftItems, currentData);

            List<ShiftRowInfo> changedInfos = UpdateRowInfos(shiftRowInfos);

            if (UseDatabases) UpdateShiftRows(changedInfos);

            // Send List of Shift Info to other Plugins
            SendShiftRowInfos(changedInfos);
        }

        // Returns list of CHANGED ShiftRowInfo objects and updates rowInfos
        List<ShiftRowInfo> UpdateRowInfos(List<ShiftRowInfo> newInfos)
        {
            List<ShiftRowInfo> Result = new List<ShiftRowInfo>();

            foreach (ShiftRowInfo newInfo in newInfos)
            {
                if (rowInfos != null)
                {
                    ShiftRowInfo info = rowInfos.Find(x => x.date == newInfo.date && x.shift == newInfo.shift && x.segmentId == newInfo.segmentId);
                    if (info == null)
                    {
                        info = new ShiftRowInfo();
                        info.id = newInfo.id;
                        info.date = newInfo.date;
                        info.shift = newInfo.shift;
                        info.segmentId = newInfo.segmentId;
                        info.start = newInfo.start;
                        info.end = newInfo.end;
                        info.start_utc = newInfo.start_utc;
                        info.end_utc = newInfo.end_utc;
                        info.type = newInfo.type;

                        foreach (string column in GenEventColumns)
                        {
                            GenEventRowInfo emptyGeri = new GenEventRowInfo();
                            emptyGeri.columnName = column;
                            info.genEventRowInfos.Add(emptyGeri);
                        }
                    }

                    info.totalTime = newInfo.totalTime;

                    // Update GenEventRowInfos
                    foreach (GenEventRowInfo geri in newInfo.genEventRowInfos)
                    {
                        GenEventRowInfo originalGeri = info.genEventRowInfos.Find(x => x.columnName == geri.columnName);
                        if (originalGeri != null)
                        {
                            originalGeri.seconds += geri.seconds;
                        }
                        else
                        {
                            GenEventRowInfo newGeri = new GenEventRowInfo();
                            newGeri.columnName = geri.columnName;
                            newGeri.seconds += geri.seconds;
                            info.genEventRowInfos.Add(newGeri);
                        }
                    }

                    Result.Add(info);
                    rowInfos.Add(info);
                }
            }

            return Result;

        }

        // Used to hold previous rowinfos
        List<ShiftRowInfo> rowInfos;

        void SendShiftRowInfos(List<ShiftRowInfo> infos)
        {
            var data = new EventData();
            data.id = "ShiftTable_ShiftRowInfos";
            data.data01 = configuration;
            data.data02 = infos;
            if (SendData != null) SendData(data);
        }

        void SendGenEventShiftItems(List<GenEventShiftItem> items)
        {
            var data = new EventData();
            data.id = "ShiftTable_GenEventShiftItems";
            data.data01 = configuration;
            data.data02 = items;
            if (SendData != null) SendData(data);
        }

        #endregion

        #endregion

    }

    class SegmentShiftTimes
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime segmentStart { get; set; }
        public DateTime segmentEnd { get; set; }
        public ShiftDate shiftData { get; set; }

        public static SegmentShiftTimes Get(DateTime d1, DateTime d2, ShiftDate date, Segment segment)
        {
            // Convert times to Local
            DateTime start = d1.ToLocalTime();
            DateTime end = d2.ToLocalTime();

            // Get DateTime objects from ShiftTime and ShiftDate along with segment.dayOffset
            DateTime segmentStart = Tools.GetDateTimeFromShiftTime(segment.beginTime, date, segment.beginDayOffset);
            DateTime segmentEnd = Tools.GetDateTimeFromShiftTime(segment.endTime, date, segment.endDayOffset);

            // Account for cases such where start & end are not during the same day as segmentStart and segmentEnd
            // Usually where dayOffset for segment is > 0 and segmentduringtype = 3;
            // Example ----------------
            // start        = 12:05 AM 9/28/2015
            // end          = 12:10 AM 9/28/2015
            // segmentStart = 12:00 AM 9/29/2015
            // segmentEnd   = 12:30 AM 9/29/2015
            // ------------------------
            if ((new ShiftDate(start, false) == new ShiftDate(end, false) && (new ShiftDate(start, false)) != new ShiftDate(segmentStart, false) && new ShiftDate(end, false) != new ShiftDate(segmentEnd, false)))
            {
                segmentStart = segmentStart.Subtract(new TimeSpan(24, 0, 0));
                segmentEnd = segmentEnd.Subtract(new TimeSpan(24, 0, 0));
            }

            // Account for cases such where End is the next day compared to Start
            // Example ----------------
            // segmentStart = 11:00 AM 9/28/2015
            // segmentEnd   = 12:00 AM 9/28/2015
            // ------------------------
            if (segmentEnd < segmentStart) { segmentEnd = segmentEnd.AddDays(1); }

            SegmentShiftTimes Result = new SegmentShiftTimes();
            Result.start = start;
            Result.end = end;
            Result.segmentStart = segmentStart;
            Result.segmentEnd = segmentEnd;

            return Result;
        }
    }
}

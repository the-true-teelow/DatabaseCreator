﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Globalization;

namespace DatabaseCreator
{
    

    /// <summary>
    /// USAGE: 
    /// - edit "databaseValues.csv" and place it in the same folder as the exe-file
    /// - add images
    /// - run "DatabseCreator.exe" and DB File will be created
    /// </summary>
    class Program
    {
        private static String typeDescriptionTableCommand = "CREATE TABLE `TypeDescriptionTable` (" +
    "`item_id`	INTEGER PRIMARY KEY AUTOINCREMENT," +
    "`name`	TEXT NOT NULL," +
    "`category`	TEXT," +
    "`distillery`	TEXT," +
    "`country`	TEXT," +
    "`description`	TEXT," +
    "`age`	INTEGER," +
    "`vol`	FLOAT," +
    "`picture_id`	INTEGER," +
    "`creationDate`	TEXT," +
    "FOREIGN KEY(`item_id`) REFERENCES `GlobalRatingTable`(`item_id`)" +
")";

        private static String pictureTableCommand = "CREATE TABLE `PictureTable` (" +
    "`item_id`	INTEGER NOT NULL," +
    "`pictureId`	INTEGER PRIMARY KEY AUTOINCREMENT," +
    "`pictureData`	TEXT," +
    "`isPicturePrivate`	INTEGER DEFAULT 0," +
    "`pictureRating`	INTEGER DEFAULT 0," +
   "FOREIGN KEY(`item_id`) REFERENCES `GlobalRatingTable`(`item_id`)" +
")";

        private static String barcodeTableCommand = "CREATE TABLE `BarcodeTable` (" +
    "`item_id`	INTEGER NOT NULL," +
    "`barcode_id` INTEGER PRIMARY KEY AUTOINCREMENT," +
    "`barcode`	TEXT NOT NULL, " +
    "FOREIGN KEY(`item_id`) REFERENCES `GlobalRatingTable`(`item_id`)" +
")";

        /// <summary>
        /// The Object, which describes the Data, which shall be stored in the Database. Correspoonds to the Database-Columns.
        /// </summary>
        struct WhiskyObjectEntry
        {
            public String name;
            public String category;
            public String distillery;
            public String country;
            public String description;
            public String barcode;
            public int age;
            public float vol;
            public String creationDate;
            public String pictureData;
        }


        static void Main(string[] args)
        {
            // read out csv file
            List<WhiskyObjectEntry> entries = ReadCsvFile();
            // write database entries according csv parameters
            createDatabaseTable(entries);

        }

        /// <summary>
        /// Reads the CSV-File, which contains the initial Values of the Database entries.
        /// </summary>
        /// <returns>List with entries which have been read out of the file</returns>
        static List<WhiskyObjectEntry> ReadCsvFile()
        {
            List<WhiskyObjectEntry> entries = new List<WhiskyObjectEntry>();
            // open file
            using (var reader = new StreamReader(@"databaseValues.csv"))
            {
                while (!reader.EndOfStream)
                {
                    WhiskyObjectEntry helperEntry = new WhiskyObjectEntry();
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    // Content specific values
                    helperEntry.name = values[0];
                    helperEntry.category = values[1];
                    helperEntry.distillery = values[2];
                    helperEntry.country = values[3];
                    helperEntry.description = values[4];
                    helperEntry.barcode = values[5];
                    helperEntry.age = Int32.Parse(values[6]);
                    helperEntry.vol = float.Parse(values[7], CultureInfo.CurrentCulture);

                    // Timestamp
                    DateTime currentTime = DateTime.Now.ToUniversalTime();
                    helperEntry.creationDate = currentTime.Year + "-" + currentTime.Month + "-" + currentTime.Day + " " + currentTime.Hour + ":" + currentTime.Minute + " UTC"; // values[8];
                    // convert image to Base64
                    helperEntry.pictureData = ImageOperations.ImageToBase64(values[9]);
                    entries.Add(helperEntry);
                }
            }      

            return entries;
        }

        
        /// <summary>
        /// Creates a database-file with corresponding entries
        /// </summary>
        /// <param name="entries">the entries, which shall be stored. The format of the entries correspond to the Object-Type of the List entries</param>
        static void createDatabaseTable(List<WhiskyObjectEntry> entries)
        {
            // Database specific information
            String databaseName = "whiskyDataDB.db";
            int versionNo = 3;
            // --------------------------------------

            int itemId = 0;
            String getRow = "";
            SQLiteDataReader reader;
            SQLiteConnection.CreateFile(databaseName);

            SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + databaseName + "; Version=" + versionNo + "; "); 
            m_dbConnection.Open();
     
            // create tables
            SQLiteCommand command = new SQLiteCommand(typeDescriptionTableCommand, m_dbConnection);
            command.ExecuteNonQuery();
            command = new SQLiteCommand(barcodeTableCommand, m_dbConnection);
            command.ExecuteNonQuery();
            command = new SQLiteCommand(pictureTableCommand, m_dbConnection);
            command.ExecuteNonQuery();

            foreach (WhiskyObjectEntry entry in entries)
            {
                // create entry in TypeDescriptionTable
                String sqlFill = "insert into TypeDescriptionTable (name, category, distillery, country, description, age, vol, picture_id, creationDate) values ('" + 
                    entry.name + "', '" +
                    entry.category + "', '" +
                    entry.distillery + "', '" +
                    entry.country + "', '" +
                    entry.description + "', '" +
                    entry.age + "', '" +
                    entry.vol.ToString("####00.0") + "', '" +
                    0 + "', '" + // pictureId, not used
                    entry.creationDate + "')";
                command = new SQLiteCommand(sqlFill, m_dbConnection);
                command.ExecuteNonQuery();

                // get latest entry from TypeDescriptionTable
                getRow = "select * from TypeDescriptionTable";
                command = new SQLiteCommand(getRow, m_dbConnection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    object helper = reader["item_id"];
                    itemId = Convert.ToInt32(helper);
                }

                // create entry in PictureTable, if picture available
                if (entry.pictureData != null && entry.pictureData != "")
                {
                    sqlFill = "insert into PictureTable (item_id, pictureData, isPicturePrivate, pictureRating) values ('" + itemId + "', '" + entry.pictureData + "', '0', '0')";
                    command = new SQLiteCommand(sqlFill, m_dbConnection);
                    command.ExecuteNonQuery();
                }

                // create entry in Barcode Table, if barcode available
                if (entry.barcode != null && entry.barcode != "")
                {
                    sqlFill = "insert into BarcodeTable (item_id, barcode) values ('" + itemId + "', '" + entry.barcode + "')";
                    command = new SQLiteCommand(sqlFill, m_dbConnection);
                    command.ExecuteNonQuery();
                }
            }

            m_dbConnection.Close();
        }
    }



    
}
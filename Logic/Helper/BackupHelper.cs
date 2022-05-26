using Hangfire;
using Ionic.Zip;
using Logic.IHelper;
using Logic.Services;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace Logic.Helper
{
    public class BackupHelper : IBackupHelper
    {
         private readonly IGeneralConfiguration _generalConfiguration;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BackupHelper(IGeneralConfiguration generalConfiguration, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _generalConfiguration = generalConfiguration;
            _emailService = emailService;
            _hostEnvironment = webHostEnvironment;
        }
     
        public void Start12()
        {
            RecurringJob.AddOrUpdate(() => AutoBackUpZippedFiles(), _generalConfiguration.Time, TimeZoneInfo.Utc);
        }



                        ///////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///                                                                                                 ///
                        /// BOTH METHODS WORKS. THE FIRST WORK BEST FOR SINGLE FILE, WHY THE SECOND WORKS BEST FOR FOLDER   ///
                        ///                                                                                                 ///
                        //////////////////////////////////////////////////////////////////////////////////////////////////////



        //public void AutoBackUpZippedFiles()
        //{
        //    var currentDate = DateTime.Now.TimeOfDay.ToString();
        //    currentDate = currentDate.Replace(":", "").Trim();
        //    string outputFile = @"C:\Users\Lord Fab Marv\Desktop\BackupFileFolder" + currentDate + ".zip";
        //    string fileToZip =_generalConfiguration.FolderPath;
        //    var todaysDate = DateTime.Today.Date;
        //    var sendTo = _generalConfiguration.AdminEmail;
        //    var subject = "BackUp File for Today" + todaysDate;
        //    string[] fileNames = Directory.GetFiles(fileToZip);
        //    using (var archive = ZipFile.Open(outputFile, ZipArchiveMode.Create))
        //    {
        //        archive.CreateEntryFromFile(fileToZip, Path.GetFileName(fileToZip));
        //        _emailService.SendEmailWithBackedUpFile(sendTo, subject, outputFile);
        //    }
        //}



        public void AutoBackUpZippedFiles()
        {
            var currentDate = DateTime.Now.TimeOfDay.ToString();
            currentDate = currentDate.Replace(":", "").Trim();
            var sendTo = _generalConfiguration.AdminEmail;
            var todaysDate = DateTime.Today.Date;
            var subject = "BackUp File for Today" + todaysDate;
            string fileToZip = _generalConfiguration.FolderPath;
            var fileNames = Directory.GetFiles(fileToZip);
            string zippedFolder = @"C:\Users\Lord Fab Marv\Desktop\BackUpFolder" + currentDate + ".zip";
            using (ZipFile zip = new())
            {
                zip.AddFiles(fileNames, "BackUpFolder");//Zip file inside filename
                zip.Save(zippedFolder);
                _emailService.SendEmailWithBackedUpFile(sendTo, subject, zippedFolder);
            }
        }
    }
}

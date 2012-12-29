﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using ITinTheDWebSite.Models;
using System.IO;
using System.Data.SqlClient;
using System.Web.Security;

namespace ITinTheDWebSite.Helpers
{
    public static class DatabaseHelper
    {
        //====================================================================================
        //      Add a user to role.                                                         //
        //                                                                                  //
        //      Adds user to the role passed by the function if they are not in that        //
        //      role already.                                                               //
        //====================================================================================

        public static void AddUserToRole(string user, string role)
        {
            var roles = (SimpleRoleProvider)Roles.Provider;

            string[] usrs = new string[] { user };
            string[] r = new string[] { role };

            // If user is not in the role passed by function then add them in it.

            if (!roles.IsUserInRole(user, role)) roles.AddUsersToRoles(usrs, r);
        }

        //==================================================================================//
        //      Store Admin information                                                     //
        //                                                                                  //
        //      Register the admin information if it is empty. Otherwise edit it.           //
        //                                                                                  //
        //      Note: If the edit is true then the account is being edited.                 //
        //      Otherwise it is being registered.                                           //
        //==================================================================================//

        public static bool StoreAdminData(RegisterModel regAdmin, ref bool edit)
        {
            int UserId = WebSecurity.GetUserId(regAdmin.EmailAddress);

            edit = false;

            try
            {
                SiteAdmin CurrentAdmin;

                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var AdminData = from r in context.SiteAdmin
                                    where r.UserId == UserId
                                    select r;

                    // If the user has some information then edit it.
                    // Otherwise register the account.

                    if (AdminData.Count() > 0 && UserId > 0)
                    {
                        CurrentAdmin = AdminData.FirstOrDefault();
                        CurrentAdmin.Company = regAdmin.CompanyName;
                        CurrentAdmin.EmailAddress = regAdmin.EmailAddress;
                        CurrentAdmin.Name = regAdmin.Name;
                        CurrentAdmin.Telephone = regAdmin.Telephone;
                        CurrentAdmin.UserId = UserId;

                        edit = true;
                    }

                    else
                    {
                        CurrentAdmin = new SiteAdmin();

                        CurrentAdmin.Company = regAdmin.CompanyName;
                        CurrentAdmin.Name = regAdmin.Name;
                        CurrentAdmin.EmailAddress = regAdmin.EmailAddress;
                        CurrentAdmin.Telephone = regAdmin.Telephone;

                        context.AddToSiteAdmin(CurrentAdmin);
                    }

                    try
                    {
                        // If the account is edited then save changes. Otherwise register the account.

                        if (edit == false)
                        {
                            WebSecurity.CreateUserAndAccount(regAdmin.EmailAddress, regAdmin.Password);

                            DatabaseHelper.AddUserToRole(regAdmin.EmailAddress, "Admin");

                            CurrentAdmin.UserId = WebSecurity.GetUserId(regAdmin.EmailAddress);
                        }

                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message;

                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Get Admin information                                                       //
        //                                                                                  //
        //      Gets the admin information if it is not empty.                              //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static RegisterModel GetAdminData(RegisterModel regAdmin, int UserId)
        {
            // If the User ID is -1 then it is being checked out by the user. We will then
            // get the current user ID.

            if (UserId == -1)
            {
                UserId = WebSecurity.CurrentUserId;
            }

            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var currentAdmin = from r in context.SiteAdmin
                                       where r.UserId == UserId
                                       select r;

                    // If the user has some information then edit it.
                    // Otherwise return nothing.

                    if (currentAdmin.Count() > 0)
                    {
                        regAdmin.Name = currentAdmin.FirstOrDefault().Name;
                        regAdmin.EmailAddress = currentAdmin.FirstOrDefault().EmailAddress;
                        regAdmin.CompanyName = currentAdmin.FirstOrDefault().Company;
                        regAdmin.Telephone = currentAdmin.FirstOrDefault().Telephone;
                        regAdmin.EmailAddress = currentAdmin.FirstOrDefault().EmailAddress;

                        // Return the modal that is filled with information from the database.

                        return (regAdmin);
                    }

                    else
                    {
                        return (null);
                    }
                }
            }

            catch
            {
                return (null);
            }
        }

        //==================================================================================//
        //      Remove Admin information                                                    //
        //                                                                                  //
        //      Removes the admin information if it is not empty.                           //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static bool RemoveAdminData(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var AdminData = from r in context.SiteAdmin
                                    where r.UserId == UserId
                                    select r;

                    // If the user has some information then remove it.
                    // Otherwise return false.

                    if (AdminData.Count() > 0 && UserId > 0)
                    {
                        context.DeleteObject(AdminData.FirstOrDefault());
                    }

                    else
                    {
                        return false;
                    }

                    try
                    {
                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Store Sponsor information                                                   //
        //                                                                                  //
        //      Register the sponsor information if it is empty. Otherwise edit it.         //
        //                                                                                  //
        //      Note: If the edit is true then the account is being edited.                 //
        //      Otherwise it is being registered.                                           //
        //==================================================================================//

        public static bool StoreSponsorData(SponsorModel sponsor, ref bool edit)
        {
            int UserId = WebSecurity.GetUserId(sponsor.EmailAddress);

            edit = false;

            try
            {
                ProspectiveCorporateSponsor CurrentSponsor;

                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var SponsorData = from r in context.ProspectiveCorporateSponsor
                                      where r.SponsorId == UserId
                                      select r;

                    // If the user has some information then edit it.
                    // Otherwise register the account.

                    if (SponsorData.Count() > 0 && UserId > 0)
                    {
                        CurrentSponsor = SponsorData.FirstOrDefault();
                        CurrentSponsor.CompanyAddress = sponsor.CompanyAddress;
                        CurrentSponsor.CompanyName = sponsor.CompanyName;
                        CurrentSponsor.ContactName = sponsor.ContactName;
                        CurrentSponsor.EmailAddress = sponsor.EmailAddress;
                        CurrentSponsor.Title = sponsor.Title;
                        CurrentSponsor.Telephone = sponsor.Telephone;
                        CurrentSponsor.Reason = sponsor.Reason;
                        CurrentSponsor.SponsorId = UserId;

                        edit = true;
                    }
                    else
                    {
                        CurrentSponsor = new ProspectiveCorporateSponsor();

                        CurrentSponsor.Status = (int)SponsorStatus.Initial;
                        CurrentSponsor.CompanyAddress = sponsor.CompanyAddress;
                        CurrentSponsor.CompanyName = sponsor.CompanyName;
                        CurrentSponsor.ContactName = sponsor.ContactName;
                        CurrentSponsor.EmailAddress = sponsor.EmailAddress;
                        CurrentSponsor.Title = sponsor.Title;
                        CurrentSponsor.Telephone = sponsor.Telephone;
                        CurrentSponsor.Reason = sponsor.Reason;

                        context.AddToProspectiveCorporateSponsor(CurrentSponsor);
                    }

                    try
                    {
                        // If the account is edited then save changes. Otherwise register the account.

                        if (edit == false)
                        {
                            WebSecurity.CreateUserAndAccount(sponsor.EmailAddress, sponsor.Password);

                            // User is automatically logged in here.

                            WebSecurity.Login(sponsor.EmailAddress, sponsor.Password);

                            DatabaseHelper.AddUserToRole(sponsor.EmailAddress, "Sponsor");

                            CurrentSponsor.SponsorId = WebSecurity.GetUserId(sponsor.EmailAddress);
                        }

                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        string errorMessage = e.Message;

                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Get Sponsor information                                                     //
        //                                                                                  //
        //      Gets the sponsor information if it is not empty.                            //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static SponsorModel GetSponsorData(SponsorModel spons, int UserId)
        {
            // If the User ID is -1 then it is being checked out by the user. We will then
            // get the current user ID.

            if (UserId == -1)
            {
                UserId = WebSecurity.CurrentUserId;
            }

            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var corporatesponsor = from r in context.ProspectiveCorporateSponsor
                                           where r.SponsorId == UserId
                                           select r;

                    // If the user has some information then edit it.
                    // Otherwise return nothing.

                    if (corporatesponsor.Count() > 0)
                    {
                        spons.CompanyName = corporatesponsor.FirstOrDefault().CompanyName;
                        spons.CompanyAddress = corporatesponsor.FirstOrDefault().CompanyAddress;
                        spons.ContactName = corporatesponsor.FirstOrDefault().ContactName;
                        spons.Title = corporatesponsor.FirstOrDefault().Title;
                        spons.Telephone = corporatesponsor.FirstOrDefault().Telephone;
                        spons.EmailAddress = corporatesponsor.FirstOrDefault().EmailAddress;
                        spons.Reason = corporatesponsor.FirstOrDefault().Reason;

                        // Return the modal that is filled with information from the database.

                        return (spons);
                    }
                    else
                    {
                        return (null);
                    }
                }
            }
            catch
            {
                return (null);
            }
        }

        //==================================================================================//
        //      Remove Sponsor information                                                  //
        //                                                                                  //
        //      Removes the sponsor information if it is not empty.                         //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static bool RemoveSponsorData(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var SponsorData = from r in context.ProspectiveCorporateSponsor
                                      where r.SponsorId == UserId
                                      select r;

                    // If the user has some information then remove it.
                    // Otherwise return false.

                    if (SponsorData.Count() > 0 && UserId > 0)
                    {
                        context.DeleteObject(SponsorData.FirstOrDefault());
                    }

                    else
                    {
                        return false;
                    }

                    try
                    {
                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Store Academic information                                                  //
        //                                                                                  //
        //      Register the academic information if it is empty. Otherwise edit it.        //
        //                                                                                  //
        //      Note: If the edit is true then the account is being edited.                 //
        //      Otherwise it is being registered.                                           //
        //==================================================================================//

        public static bool StoreAcademicData(AcademicModel academic, ref bool edit)
        {
            int UserId = WebSecurity.CurrentUserId;

            edit = false;

            try
            {
                ProspectiveAcademic CurrentAcademic;

                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var AcademicData = from r in context.ProspectiveAcademic
                                       where r.AcademicId == UserId
                                       select r;

                    // If the user has some information then edit it.
                    // Otherwise register the account.

                    if (AcademicData.Count() > 0 && UserId > 0)
                    {
                        CurrentAcademic = AcademicData.FirstOrDefault();
                        CurrentAcademic.AcademyAddress = academic.AcademyAddress;
                        CurrentAcademic.AcademyName = academic.AcademyName;
                        CurrentAcademic.PrimaryContactName = academic.PrimaryContactName;
                        CurrentAcademic.PrimaryEmailAddress = academic.PrimaryEmailAddress;
                        CurrentAcademic.PrimaryTitle = academic.PrimaryTitle;
                        CurrentAcademic.PrimaryTelephone = academic.PrimaryTelephone;

                        CurrentAcademic.SecondaryContactName = academic.SecondaryContactName;
                        CurrentAcademic.SecondaryEmailAddress = academic.SecondaryEmailAddress;
                        CurrentAcademic.SecondaryTitle = academic.SecondaryTitle;
                        CurrentAcademic.SecondaryTelephone = academic.SecondaryTelephone;

                        CurrentAcademic.AcademicId = UserId;

                        edit = true;
                    }

                    else
                    {
                        CurrentAcademic = new ProspectiveAcademic();

                        CurrentAcademic.Status = (int)AcademicStatus.Initial;
                        CurrentAcademic.AcademyAddress = academic.AcademyAddress;
                        CurrentAcademic.AcademyName = academic.AcademyName;
                        CurrentAcademic.PrimaryContactName = academic.PrimaryContactName;
                        CurrentAcademic.PrimaryEmailAddress = academic.PrimaryEmailAddress;
                        CurrentAcademic.PrimaryTitle = academic.PrimaryTitle;
                        CurrentAcademic.PrimaryTelephone = academic.PrimaryTelephone;

                        CurrentAcademic.SecondaryContactName = academic.SecondaryContactName;
                        CurrentAcademic.SecondaryEmailAddress = academic.SecondaryEmailAddress;
                        CurrentAcademic.SecondaryTitle = academic.SecondaryTitle;
                        CurrentAcademic.SecondaryTelephone = academic.SecondaryTelephone;

                        context.AddToProspectiveAcademic(CurrentAcademic);
                    }

                    try
                    {
                        // If the account is edited then save changes. Otherwise register the account.

                        if (edit == false)
                        {
                            WebSecurity.CreateUserAndAccount(academic.PrimaryEmailAddress, academic.Password);
                            WebSecurity.Login(academic.PrimaryEmailAddress, academic.Password);

                            DatabaseHelper.AddUserToRole(academic.PrimaryEmailAddress, "Educator");

                            CurrentAcademic.AcademicId = WebSecurity.GetUserId(academic.PrimaryEmailAddress);
                        }

                        context.SaveChanges();
                        return true;
                    }

                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            catch
            {
                return false;
            }
        }

        //==================================================================================//
        //      Get Academic information                                                    //
        //                                                                                  //
        //      Gets the academic information if it is not empty.                           //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static AcademicModel GetAcademicdData(AcademicModel academic, int UserId)
        {
            // If the User ID is -1 then it is being checked out by the user. We will then
            // get the current user ID.

            if (UserId == -1)
            {
                UserId = WebSecurity.CurrentUserId;
            }

            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var ExistingAcademic = from r in context.ProspectiveAcademic
                                           where r.AcademicId == UserId
                                           select r;

                    // If the user has some information then edit it.
                    // Otherwise return nothing.

                    if (ExistingAcademic.Count() > 0)
                    {
                        academic.AcademyName = ExistingAcademic.FirstOrDefault().AcademyName;
                        academic.AcademyAddress = ExistingAcademic.FirstOrDefault().AcademyAddress;
                        academic.PrimaryContactName = ExistingAcademic.FirstOrDefault().PrimaryContactName;
                        academic.PrimaryTitle = ExistingAcademic.FirstOrDefault().PrimaryTitle;
                        academic.PrimaryTelephone = ExistingAcademic.FirstOrDefault().PrimaryTelephone;
                        academic.PrimaryEmailAddress = ExistingAcademic.FirstOrDefault().PrimaryEmailAddress;

                        academic.SecondaryContactName = ExistingAcademic.FirstOrDefault().SecondaryContactName;
                        academic.SecondaryTitle = ExistingAcademic.FirstOrDefault().SecondaryTitle;
                        academic.SecondaryTelephone = ExistingAcademic.FirstOrDefault().SecondaryTelephone;
                        academic.SecondaryEmailAddress = ExistingAcademic.FirstOrDefault().SecondaryEmailAddress;

                        // Return the modal that is filled with information from the database.

                        return (academic);
                    }

                    else
                    {
                        return (null);
                    }
                }
            }

            catch
            {
                return (null);
            }
        }

        //==================================================================================//
        //      Remove Academic information                                                 //
        //                                                                                  //
        //      Removes the academic information if it is not empty.                        //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static bool RemoveAcademicData(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var SponsorData = from r in context.ProspectiveAcademic
                                      where r.AcademicId == UserId
                                      select r;

                    // If the user has some information then remove it.
                    // Otherwise return false.

                    if (SponsorData.Count() > 0 && UserId > 0)
                    {
                        context.DeleteObject(SponsorData.FirstOrDefault());
                    }

                    else
                    {
                        return false;
                    }

                    try
                    {
                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Store Prospect Student information                                          //
        //                                                                                  //
        //      Register the prospective student information if it is                       //
        //      empty. Otherwise edit it.                                                   //
        //                                                                                  //
        //      Note: If the edit is true then the account is being edited.                 //
        //      Otherwise it is being registered.                                           //
        //==================================================================================//

        public static bool StoreProspectData(ProspectModel prospect, ref bool edit)
        {
            int UserId = WebSecurity.CurrentUserId;

            ProspectiveStudent CurrentStudent;

            edit = false;

            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var ProspectData = from r in context.ProspectiveStudent
                                       where r.UserId == UserId
                                       select r;

                    // If the user has some information then edit it.
                    // Otherwise register the account.

                    if (ProspectData.Count() > 0 && UserId > 0)
                    {
                        edit = true;

                        CurrentStudent = ProspectData.FirstOrDefault();
                        CurrentStudent.UserId = UserId;
                        CurrentStudent.Name = prospect.Name;
                        CurrentStudent.Telephone = prospect.Telephone;
                        CurrentStudent.EmailAddress = prospect.EmailAddress;
                        CurrentStudent.DesiredCareerPath = prospect.DesiredCareerPath;
                        CurrentStudent.Gender = prospect.Gender;

                        // Store the new resume if it is supplied and not empty.

                        if (prospect.ResumeFile != null && prospect.ResumeFile.ContentLength > 0)
                        {
                            ProspectiveStudentResume Resume = new ProspectiveStudentResume();

                            using (MemoryStream ms = new MemoryStream())
                            {
                                prospect.ResumeFile.InputStream.CopyTo(ms);

                                Resume.FileContent = ms.ToArray();
                                Resume.FileName = Path.GetFileName(prospect.ResumeFile.FileName);
                                Resume.ContentType = prospect.ResumeFile.ContentType;
                                Resume.ContentLength = prospect.ResumeFile.ContentLength;

                                DatabaseHelper.UploadResume(Resume, prospect);

                                CurrentStudent.ResumeUploaded = "Yes";
                                prospect.ResumeUploaded = "Yes";
                            }
                        }

                        // Store the new transcript if it is supplied and not empty.

                        if (prospect.TranscriptFile != null && prospect.TranscriptFile.ContentLength > 0)
                        {
                            ProspectiveStudentTranscript Transcript = new ProspectiveStudentTranscript();

                            using (MemoryStream ts = new MemoryStream())
                            {
                                prospect.TranscriptFile.InputStream.CopyTo(ts);

                                Transcript.FileContent = ts.ToArray();
                                Transcript.FileName = Path.GetFileName(prospect.TranscriptFile.FileName);
                                Transcript.ContentType = prospect.TranscriptFile.ContentType;
                                Transcript.ContentLength = prospect.TranscriptFile.ContentLength;

                                DatabaseHelper.UploadTranscript(Transcript, prospect);

                                CurrentStudent.TranscriptUploaded = "Yes";
                                prospect.TranscriptUploaded = "Yes";
                            }
                        }
                    }

                    else
                    {
                        CurrentStudent = new ProspectiveStudent();

                        CurrentStudent.Status = (int)StudentStatus.Initial;
                        CurrentStudent.Name = prospect.Name;
                        CurrentStudent.Telephone = prospect.Telephone;
                        CurrentStudent.EmailAddress = prospect.EmailAddress;
                        CurrentStudent.DesiredCareerPath = prospect.DesiredCareerPath;
                        CurrentStudent.Gender = prospect.Gender;

                        context.AddToProspectiveStudent(CurrentStudent);
                    }

                    try
                    {
                        // If the account is edited then save changes. Otherwise register the account.

                        if (edit == false)
                        {
                            WebSecurity.CreateUserAndAccount(prospect.EmailAddress, prospect.Password);

                            // User is automatically logged in here.

                            WebSecurity.Login(prospect.EmailAddress, prospect.Password);

                            DatabaseHelper.AddUserToRole(prospect.EmailAddress, "Student");

                            CurrentStudent.UserId = WebSecurity.GetUserId(prospect.EmailAddress);

                            // Store the resume if it is supplied and not empty.

                            if (prospect.ResumeFile != null && prospect.ResumeFile.ContentLength > 0)
                            {
                                ProspectiveStudentResume Resume = new ProspectiveStudentResume();

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    prospect.ResumeFile.InputStream.CopyTo(ms);

                                    Resume.FileContent = ms.ToArray();
                                    Resume.FileName = Path.GetFileName(prospect.ResumeFile.FileName);
                                    Resume.ContentType = prospect.ResumeFile.ContentType;
                                    Resume.ContentLength = prospect.ResumeFile.ContentLength;

                                    DatabaseHelper.UploadResume(Resume, prospect);

                                    CurrentStudent.ResumeUploaded = "Yes";
                                    prospect.ResumeUploaded = "Yes";
                                }
                            }

                            else
                            {
                                CurrentStudent.ResumeUploaded = "No";
                                prospect.ResumeUploaded = "No";
                            }

                            // Store the resume if it is supplied and not empty.

                            if (prospect.TranscriptFile != null && prospect.TranscriptFile.ContentLength > 0)
                            {
                                ProspectiveStudentTranscript Transcript = new ProspectiveStudentTranscript();

                                using (MemoryStream ts = new MemoryStream())
                                {
                                    prospect.TranscriptFile.InputStream.CopyTo(ts);

                                    Transcript.FileContent = ts.ToArray();
                                    Transcript.FileName = Path.GetFileName(prospect.TranscriptFile.FileName);
                                    Transcript.ContentType = prospect.TranscriptFile.ContentType;
                                    Transcript.ContentLength = prospect.TranscriptFile.ContentLength;

                                    DatabaseHelper.UploadTranscript(Transcript, prospect);

                                    CurrentStudent.TranscriptUploaded = "Yes";
                                    prospect.TranscriptUploaded = "Yes";
                                }
                            }

                            else
                            {
                                CurrentStudent.TranscriptUploaded = "No";
                                prospect.TranscriptUploaded = "No";
                            }
                        }

                        context.SaveChanges();
                        return true;
                    }

                    catch
                    {
                        return false;
                    }
                }
            }

            catch
            {
                return false;
            }
        }

        //==================================================================================//
        //      Get Prospect Student information                                            //
        //                                                                                  //
        //      Gets the prospect student information if it is not empty.                   //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static ProspectModel GetProspectData(ProspectModel prospect, int UserId)
        {
            // If the User ID is -1 then it is being checked out by the user. We will then
            // get the current user ID.

            if (UserId == -1)
            {
                UserId = WebSecurity.CurrentUserId;
            }
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var ExistingProspect = from r in context.ProspectiveStudent
                                           where r.UserId == UserId
                                           select r;

                    // If the user has some information then edit it.
                    // Otherwise return nothing.

                    if (ExistingProspect.Count() > 0)
                    {
                        prospect.Name = ExistingProspect.FirstOrDefault().Name;
                        prospect.Telephone = ExistingProspect.FirstOrDefault().Telephone;
                        prospect.EmailAddress = ExistingProspect.FirstOrDefault().EmailAddress;
                        prospect.DesiredCareerPath = ExistingProspect.FirstOrDefault().DesiredCareerPath;
                        prospect.Gender = ExistingProspect.FirstOrDefault().Gender;
                        prospect.ResumeUploaded = ExistingProspect.FirstOrDefault().ResumeUploaded;
                        prospect.TranscriptUploaded = ExistingProspect.FirstOrDefault().TranscriptUploaded;

                        // Return the modal that is filled with information from the database.

                        return (prospect);
                    }

                    else
                    {
                        return (null);
                    }
                }
            }

            catch
            {
                return (null);
            }

        }

        //==================================================================================//
        //      Remove Prospect Student information                                         //
        //                                                                                  //
        //      Removes the prospective student information if it is not empty.             //
        //                                                                                  //
        //      Note: If the User ID is -1 then it is being checked out by the user.        //
        //      If not then it is being checked by the admin.                               //
        //==================================================================================//

        public static bool RemoveProspectiveData(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var SponsorData = from r in context.ProspectiveStudent
                                      where r.UserId == UserId
                                      select r;

                    // If the user has some information then remove it.
                    // Otherwise return false.

                    if (SponsorData.Count() > 0 && UserId > 0)
                    {
                        context.DeleteObject(SponsorData.FirstOrDefault());
                    }

                    else
                    {
                        return false;
                    }

                    try
                    {
                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Upload Prospect Student Transcript.                                         //
        //                                                                                  //
        //      Uploads the prospective student transcript if it is not empty.              //
        //==================================================================================//

        public static bool UploadTranscript(ProspectiveStudentTranscript f, ProspectModel prospect)
        {
            int UserId = WebSecurity.GetUserId(prospect.EmailAddress);

            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var UserTranscript = from r in context.ProspectiveStudentTranscripts
                                         where r.UserId == UserId
                                         select r;

                    // If the user has a transcript then update it.
                    // Otherwise make a new row in the database.

                    if (UserTranscript.Count() > 0)
                    {
                        ProspectiveStudentTranscript currentTranscript = UserTranscript.FirstOrDefault();

                        currentTranscript.UserId = UserId;
                        currentTranscript.FileContent = f.FileContent;
                        currentTranscript.FileName = f.FileName;
                        currentTranscript.ContentLength = f.ContentLength;

                        context.SaveChanges();

                        return true;
                    }

                    else
                    {
                        f.UserId = UserId;
                        context.AddToProspectiveStudentTranscripts(f);
                        context.SaveChanges();
                        return true;
                    }
                }
            }

            catch (Exception)
            {
                return false;
            }
        }

        //==================================================================================//
        //      Get Prospect Student Transcript.                                            //
        //                                                                                  //
        //      Gets the prospective student transcript if it is not empty.                 //
        //==================================================================================//

        public static ProspectiveStudentTranscript GetTranscript(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var transcript = from r in context.ProspectiveStudentTranscripts
                                     where r.UserId == UserId
                                     select r;

                    // If the user has a transcript then return it.
                    // Otherwise return nothing.

                    if (transcript.Count() > 0)
                    {
                        return (transcript.FirstOrDefault());
                    }

                    else
                    {
                        return null;
                    }
                }
            }

            catch
            {
                return (null);
            }
        }

        //==================================================================================//
        //      Remove Prospect Student Transcript.                                         //
        //                                                                                  //
        //      Removes the prospective student transcript if it is not empty.              //
        //==================================================================================//

        public static bool RemoveTranscript(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var UserTranscript = from r in context.ProspectiveStudentTranscripts
                                         where r.UserId == UserId
                                         select r;

                    // If the user has a transcript then update it.
                    // Otherwise make a new row in the database.

                    if (UserTranscript.Count() > 0 && UserId > 0)
                    {
                        context.DeleteObject(UserTranscript.FirstOrDefault());
                    }

                    else
                    {
                        return false;
                    }

                    try
                    {
                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }

        //==================================================================================//
        //      Upload Prospect Student Resume.                                             //
        //                                                                                  //
        //      Uploads the prospective student transcript if it is not empty.              //
        //==================================================================================//

        public static bool UploadResume(ProspectiveStudentResume resume, ProspectModel prospect)
        {
            int UserId = WebSecurity.GetUserId(prospect.EmailAddress);

            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var UserResume = from r in context.ProspectiveStudentResumes
                                     where r.UserId == UserId
                                     select r;

                    // If the user has a resume then update it.
                    // Otherwise make a new row in the database.

                    if (UserResume.Count() > 0)
                    {
                        ProspectiveStudentResume currentResume = UserResume.FirstOrDefault();

                        currentResume.UserId = UserId;
                        currentResume.FileContent = resume.FileContent;
                        currentResume.FileName = resume.FileName;
                        currentResume.ContentLength = resume.ContentLength;
                        context.SaveChanges();
                        return true;
                    }

                    else
                    {
                        resume.UserId = UserId;
                        context.AddToProspectiveStudentResumes(resume);
                        context.SaveChanges();
                        return true;
                    }
                }
            }

            catch (Exception)
            {
                return false;
            }
        }

        //==================================================================================//
        //      Get Prospect Student Resume.                                                //
        //                                                                                  //
        //      Gets the prospective student resume if it is not empty.                     //
        //==================================================================================//

        public static ProspectiveStudentResume GetResume(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var resume = from r in context.ProspectiveStudentResumes
                                 where r.UserId == UserId
                                 select r;

                    // If the user has a resume then return it.
                    // Otherwise return nothing.

                    if (resume.Count() > 0)
                    {
                        //return File(resume.FirstOrDefault().FileContent, resume.FirstOrDefault().ContentType);
                        return (resume.FirstOrDefault());
                    }

                    else
                    {
                        return null;
                    }
                }
            }

            catch
            {
                return (null);
            }
        }

        //==================================================================================//
        //      Remove Prospect Student Resume.                                             //
        //                                                                                  //
        //      Removes the prospective student resume if it is not empty.                  //
        //==================================================================================//

        public static bool RemoveResume(int UserId)
        {
            try
            {
                using (ITintheDTestTableEntities context = new ITintheDTestTableEntities())
                {
                    // Put everything we find in the database in the var variable. All the
                    // information will be gotten using the User ID.

                    var UserResume = from r in context.ProspectiveStudentResumes
                                     where r.UserId == UserId
                                     select r;

                    // If the user has a resume then update it.
                    // Otherwise make a new row in the database.

                    if (UserResume.Count() > 0 && UserId > 0)
                    {
                        context.DeleteObject(UserResume.FirstOrDefault());
                    }

                    else
                    {
                        return false;
                    }

                    try
                    {
                        context.SaveChanges();

                        return true;
                    }

                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                string exMessage = ex.Message;

                return false;
            }
        }
    }
}
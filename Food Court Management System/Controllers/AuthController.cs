using DigitalArena.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Food_Court_Management_System.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {

        public ActionResult CustomerLogin()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult CustomerLogin(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid form submission.";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    string query = "SELECT * FROM Customer WHERE Username = :username AND Password = :password";
                    using (OracleCommand cmd = new OracleCommand(query, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("username", model.Username));
                        cmd.Parameters.Add(new OracleParameter("password", model.Password));

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string username = reader.GetString(reader.GetOrdinal("Username"));

                                // Set session and auth cookie
                                FormsAuthentication.SetAuthCookie(username, true);

                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                }

                ViewBag.Message = "Invalid username or password.";
                ViewBag.IsSuccess = false;
                return View(model);
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error: " + e.Message;
                ViewBag.IsSuccess = false;
                return View(model);
            }
        }

        public ActionResult RestaurantLogin()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult RestaurantLogin(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid form submission.";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    string query = "SELECT * FROM Restaurant WHERE EMAIL = :email AND Password = :password";
                    using (OracleCommand cmd = new OracleCommand(query, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("email", model.Username));
                        cmd.Parameters.Add(new OracleParameter("password", model.Password));

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string username = reader.GetString(reader.GetOrdinal("email"));

                                // Set session and auth cookie
                                FormsAuthentication.SetAuthCookie(username, true);

                                return RedirectToAction("Index", "RestaurantDashboard");
                            }
                        }
                    }
                }

                ViewBag.Message = "Invalid username or password.";
                ViewBag.IsSuccess = false;
                return View(model);
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error: " + e.Message;
                ViewBag.IsSuccess = false;
                return View(model);
            }
        }

        public ActionResult Logout()
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Auth");

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        
         public ActionResult Register()
         {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
         }
        [HttpPost]
        public ActionResult Register(string Username, string Email, string Password, string Phone, string Name, string Address)
        {
            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    // Step 1: Insert Customer
                    string insertCustomerQuery = @"
                INSERT INTO Customer (CustomerID, Username, Email, Password, Phone, Name, Address)
                VALUES (customer_seq.NEXTVAL, :Username, :Email, :Password, :Phone, :Name, :Address)
                RETURNING CustomerID INTO :InsertedCustomerId";

                    int newCustomerId = 0;

                    using (OracleCommand cmd = new OracleCommand(insertCustomerQuery, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("Username", Username));
                        cmd.Parameters.Add(new OracleParameter("Email", Email));
                        cmd.Parameters.Add(new OracleParameter("Password", Password));
                        cmd.Parameters.Add(new OracleParameter("Phone", Phone));
                        cmd.Parameters.Add(new OracleParameter("Name", Name));
                        cmd.Parameters.Add(new OracleParameter("Address", Address));

                        OracleParameter outId = new OracleParameter("InsertedCustomerId", OracleDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outId);

                        cmd.ExecuteNonQuery();
                        newCustomerId = Convert.ToInt32(outId.Value.ToString());
                    }

                    // Step 2: Insert Wallet with a random 6-digit PIN
                    string randomPin = (123456).ToString();

                    string insertWalletQuery = @"
                INSERT INTO Wallet (WalletID, CustomerID, Pin, Balance)
                VALUES (wallet_seq.NEXTVAL, :CustomerID, :Pin, 0)";

                    using (OracleCommand cmd = new OracleCommand(insertWalletQuery, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("CustomerID", newCustomerId));
                        cmd.Parameters.Add(new OracleParameter("Pin", randomPin));

                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Message"] = "Account created successfully. Please login.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("CustomerLogin", "Auth");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Registration failed: " + ex.Message;
                ViewBag.IsSuccess = false;
                return View();
            }
        }





        public ActionResult ForgotPassword()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Identifier)
        {
            if (string.IsNullOrWhiteSpace(Identifier))
            {
                ViewBag.Message = "Please enter a valid username or email.";
                ViewBag.IsSuccess = false;
                return View();
            }

            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    string query = @"
                SELECT CustomerID, Username, Email, Phone, Name 
                FROM Customer 
                WHERE LOWER(Username) = :identifier OR LOWER(Email) = :identifier";

                    OracleCommand cmd = new OracleCommand(query, con);
                    cmd.Parameters.Add(new OracleParameter("identifier", Identifier.ToLower()));

                    OracleDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int customerId = reader.GetInt32(reader.GetOrdinal("CustomerID"));
                        string username = reader.GetString(reader.GetOrdinal("Username"));
                        string email = reader.GetString(reader.GetOrdinal("Email"));
                        string phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone"));
                        string fullName = reader.GetString(reader.GetOrdinal("Name"));

                        // Generate OTP
                        string otp = new Random().Next(100000, 999999).ToString();

                        // Store in Session
                        Session["ResetOtp"] = otp;
                        Session["OtpUserId"] = customerId;
                        Session["OtpExpiry"] = DateTime.Now.AddMinutes(2);

                        // SMS content
                        string message = $"Dear {fullName.Split(' ')[0]},\nYour Dine Avenue Reset Password OTP is {otp}.\nThis code will expire in 2 minutes.\n\nPlease do not share this OTP with anyone.";
                        var smsService = new SmsService();
                        string smsResponse = smsService.SendSms(phone, message);

                        if (smsResponse.Contains("\"response_code\":202"))
                        {
                            ViewBag.Message = "An OTP has been sent to your registered phone number.";
                            ViewBag.IsSuccess = true;
                            return RedirectToAction("OTPVerification");
                        }
                        else
                        {
                            ViewBag.Message = "Failed to send OTP. Response: " + smsResponse;
                            ViewBag.IsSuccess = false;
                        }
                    }
                    else
                    {
                        ViewBag.Message = "No account found with that username or email.";
                        ViewBag.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An error occurred: " + ex.Message;
                ViewBag.IsSuccess = false;
            }

            return View();
        }

        public ActionResult OTPVerification()
        {
            if (Session["ResetOtp"] == null || !Regex.IsMatch(Session["ResetOtp"].ToString(), @"^\d{6}$"))
            {
                return RedirectToAction("ForgotPassword", "Auth");
            }

            return View();
        }

        [HttpPost]
        public ActionResult OTPVerification(string[] OtpDigits)
        {
            string enteredOtp = string.Join("", OtpDigits);

            if (Session["ResetOtp"] == null || Session["OtpUserId"] == null || Session["OtpExpiry"] == null)
            {
                TempData["Message"] = "Your session has expired. Please request a new OTP.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("ForgotPassword");
            }

            string storedOtp = Session["ResetOtp"].ToString();
            DateTime expiry = (DateTime)Session["OtpExpiry"];

            if (DateTime.Now > expiry)
            {
                Session.Clear();
                TempData["Message"] = "OTP has expired. Please request a new one.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("ForgotPassword");
            }

            if (enteredOtp != storedOtp)
            {
                ViewBag.Message = "Invalid OTP. Please try again.";
                ViewBag.IsSuccess = false;
                return View();
            }

            // Success
            Session["AllowPasswordResetUserId"] = Session["OtpUserId"];
            Session.Remove("ResetOtp");
            Session.Remove("OtpUserId");
            Session.Remove("OtpExpiry");

            return RedirectToAction("ResetPassword");
        }





        public ActionResult ResetPassword()
        {
            if (Session["AllowPasswordResetUserId"] == null)
                return RedirectToAction("ForgotPassword", "Auth");

            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ViewBag.Message = "Please fill in all fields.";
                ViewBag.IsSuccess = false;
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                ViewBag.IsSuccess = false;
                return View();
            }

            if (Session["AllowPasswordResetUserId"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            int customerId = (int)Session["AllowPasswordResetUserId"];

            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    string updateQuery = "UPDATE Customer SET Password = :password WHERE CustomerID = :id";
                    using (OracleCommand cmd = new OracleCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("password", NewPassword));
                        cmd.Parameters.Add(new OracleParameter("id", customerId));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Session.Remove("AllowPasswordResetUserId");
                            TempData["Message"] = "Your password has been successfully reset.";
                            TempData["IsSuccess"] = true;
                            return RedirectToAction("CustomerLogin", "Auth");
                        }
                        else
                        {
                            ViewBag.Message = "Failed to reset password.";
                            ViewBag.IsSuccess = false;
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An error occurred: " + ex.Message;
                ViewBag.IsSuccess = false;
                return View();
            }
        }

        public ActionResult AdminLogin()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult AdminLogin(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid form submission.";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                using (OracleConnection con = new OracleConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                {
                    con.Open();

                    string query = "SELECT * FROM Admin WHERE Email = :email AND Password = :password";
                    using (OracleCommand cmd = new OracleCommand(query, con))
                    {
                        cmd.Parameters.Add(new OracleParameter("email", model.Username));
                        cmd.Parameters.Add(new OracleParameter("password", model.Password));

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string username = reader.GetString(reader.GetOrdinal("Username"));
                                FormsAuthentication.SetAuthCookie(username, true);

                                return RedirectToAction("Index", "AdminDashboard");
                            }
                        }
                    }
                }

                ViewBag.Message = "Invalid email or password.";
                ViewBag.IsSuccess = false;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
                ViewBag.IsSuccess = false;
                return View(model);
            }
        }


    }
}
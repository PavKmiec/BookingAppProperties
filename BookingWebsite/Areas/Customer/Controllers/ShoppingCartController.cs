﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingWebsite.Areas.Identity.Pages.Account.Manage;
using BookingWebsite.Data;
using BookingWebsite.Models;
using BookingWebsite.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using BookingWebsite.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingWebsite.Areas.Customer.Controllers
{
    /// <summary>
    /// Shopping Cart Controller
    /// </summary>
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        // dependency injection for ApplicationDbContext
        private readonly ApplicationDbContext _db;

        private readonly IEmailSender _emailSender;
        private UserManager<IdentityUser> _userManager;




        // bind a ShoppingCartViewModel property that will be used in this controller,
        //this way we don't need to use this in parameters every time
        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        // constructor
        public ShoppingCartController(ApplicationDbContext db, IEmailSender emailSender, UserManager<IdentityUser> userManager) // IdentityUser??
        {
            _db = db;
            _emailSender = emailSender;
            _userManager = userManager;

            // initialise ShoppingCartVM
            ShoppingCartVM = new ShoppingCartViewModel()
            {
                // with list of products

                Products = new List<Models.Products>()
            };


        }
        /// <summary>
        /// Index Action - we need to retrieve cart items that are in the session based on those Id's we need to populate ViewModel
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // get session items
            List<int> lstShoppingCart = HttpContext.Session.Get<List<int>>("ssShoppingCart");
            if (lstShoppingCart.Count>0)
            {

                // reference
                //var currUser = await _userManager.GetUserAsync(User);

                //ApplicationUser appUser = new ApplicationUser
                //{
                //    Name = currUser.Name,
                //    PhoneNumber = currUser.PhoneNumber,
                //    Email = currUser.Email

                //};


                // build complete list of products so we can use that in Shopping Cart View
                foreach (int cartItem in lstShoppingCart)
                {
                    // we will also include product types and tags once we load the product
                    Products prod = _db.Products.Include(p=>p.Tags).Include(p=>p.ProductTypes).Where(p => p.Id == cartItem).FirstOrDefault();
                    ShoppingCartVM.Products.Add(prod);


                    // get current user - to use it to pre-fill user data for appointment
                    var appUser = await _userManager.GetUserAsync(User);

                    // set user for appointment from logged in user
                    ShoppingCartVM.CustomerUser = (ApplicationUser) appUser;
                }

            }
            // once we loaded products we will pass the model to the View
            return View(ShoppingCartVM);
        }


        /// <summary>
        /// Index POST action
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost() // no need for parameters because we have already bind it
        {
            //claims
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);




            // retrieve list of items from session
            List<int> lstCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            // appointments time and dte
            ShoppingCartVM.Appointments.AppointmentDate = ShoppingCartVM.Appointments.AppointmentDate
                .AddHours(ShoppingCartVM.Appointments.AppointmentTime.Hour)
                .AddMinutes(ShoppingCartVM.Appointments.AppointmentTime.Minute);



            

            // create object for appointments
            Appointments appointments = ShoppingCartVM.Appointments;

            // get date from VM for customer to save it for appointment
            appointments.CustomerName = ShoppingCartVM.CustomerUser.Name;
            appointments.CustomerEmail = ShoppingCartVM.CustomerUser.Email;
            appointments.CustomerPhoneNumber = ShoppingCartVM.CustomerUser.PhoneNumber;





            // add this appointment to database

            _db.Appointments.Add(appointments);
            _db.SaveChanges();

            // once appointment is saved we will get appointment Id which is created

            int appointmentId = appointments.Id;


            // use Id to insert records inside product selected for appointment
            foreach (int productId in lstCartItems)
            {
                ProductsSelectedForAppointment productsSelectedForAppointment = new ProductsSelectedForAppointment()
                {
                    AppointmentId = appointmentId,
                    ProductId = productId
                };

                _db.ProductsSelectedForAppointments.Add(productsSelectedForAppointment);
                

            }


            // email  - Commented out in order not to spam during testing
            //TODO this is commented to prevent sending emails during development - uncomment for production - 
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
            //    "Open Properties - Your Appointment",
            //    "Your appointment was submitted successfully, a member of staff will be in touch shortly to confirm your appointment");

            Debug.WriteLine("Email Reached");

            _db.SaveChanges();
            // empty list cart items
            lstCartItems = new List<int>();
            // set session
            HttpContext.Session.Set("ssShoppingCart", lstCartItems);

            // once done redirect to confirmation page , controller is ShoppingCart and we need to pass the the ID of appointment 
            return RedirectToAction("AppointmentConfirmation", "ShoppingCart", new {Id = appointmentId} );

        }


        // remove item from basket action
        /// <summary>
        /// Remove action method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult Remove(int id)
        {

            // get list of items from session

            List<int> lstCartItems = HttpContext.Session.Get<List<int>>("ssShoppingCart");

            // check and remove from cart
            if (lstCartItems.Count > 0)
            {
                if (lstCartItems.Contains(id))
                {
                    lstCartItems.Remove(id);
                }
                
            }
            // set session again to reflect this change
            HttpContext.Session.Set("ssShoppingCart", lstCartItems);
            // return redirect to action 
            return RedirectToAction(nameof(Index));

        }

        /// <summary>
        /// Appointment confirmation / passing appointment id as parameter 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> AppointmentConfirmation(int id)
        {

            //claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // fill shopping cart view model based of the appointment ID 
            ShoppingCartVM.Appointments = _db.Appointments.Where(a => a.Id == id).FirstOrDefault();

            // retrieve all of the products for that appointment and add to model 

            List<ProductsSelectedForAppointment> objProdList =
                _db.ProductsSelectedForAppointments.Where(p => p.AppointmentId == id).ToList();

            // iterate thought complete list, add products inside shopping  cart view model
            foreach (ProductsSelectedForAppointment prodAptObj in objProdList)
            {

                ShoppingCartVM.Products.Add(_db.Products.Include(p=>p.ProductTypes).Include(p=>p.Tags).Where(p=>p.Id == prodAptObj.ProductId).FirstOrDefault());
                
            }

            //TODO this is commented to prevent sending emails during development - uncomment for production - 
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email,
            //    "Open Properties - Your Appointment",
            //    "Your appointment was submitted successfully, a member of staff will be in touch shortly to confirm your appointment");

            //Debug.WriteLine("Email Reached");



            return View(ShoppingCartVM);

        }
    }
}
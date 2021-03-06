﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingWebsite.Data;
using BookingWebsite.Models;
using BookingWebsite.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingWebsite.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class ProductTypesController : Controller
    {
        
        /// <summary>
        /// application dbcontext object for crud operations, with .net core we using dependency injection
        /// </summary>
        private readonly ApplicationDbContext _db;


        
        /// <summary>
        /// constructor for dependency injection
        /// retrieve applicationdbcontext using dependency injection
        /// and populate this db inside readonly object 
        /// </summary>
        /// <param name="db"></param>
        public ProductTypesController(ApplicationDbContext db)
        {
            _db = db;
        }

        //[Authorize(Roles = SD.SellerEndUser + "," + SD.SuperAdminEndUser)]
        /// <summary>
        /// Index action
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            
            // using entity framework to access database and convert to list and pass it to the view
            return View(_db.ProductTypes.ToList());
        }


        /// <summary>
        /// Get Create Action
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }



        // build-in asp.net functionality
        // With each request of httpPost AntiForgeryToken is added and passes along with the request
        // Once it reaches the server it is checked if its valid (not be altered)
        /// <summary>
        /// Create POST action
        /// </summary>
        /// <param name="productTypes"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(ProductTypes productTypes)
        {
            //  Modelstate.IsValid - validation check on the server side if model is valid.
            //   we can have meany attributes assigned to properties in out models
            // asp.net will check against those attributes 
            if (ModelState.IsValid)
            {
                // if it is valid we will retrieved information to the database
                _db.Add(productTypes);
                // once addad we save changes to database
                await _db.SaveChangesAsync();
                // return to index with product types ; using "nameof" helps prevent spelling errors
                return RedirectToAction(nameof(Index));
            }

            return View(productTypes);
        }


        // GET Edit Action Method - retrieving Id which user wants to edit
        /// <summary>
        /// GET Edit action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // if id is null return nor found
            if (id == null)
            {
                return NotFound();

            }
            // if Id is not null, retrieve product type from database
            var productType = await _db.ProductTypes.FindAsync(id);
            // check if producType is null, return not found
            if (productType == null)
            {
                return NotFound();

            }
            // if producType is not null return vieW passing productType to the edit view
            return View(productType);
        }



        // build-in asp.net functionality
        // With each request of httpPost AntiForgeryToken is added and passes along with the request
        // Once it reaches the server it is checked if its valid (not be altered)
        /// <summary>
        /// POST Edit action
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productTypes"></param>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(int id, ProductTypes productTypes)
        {
            

            // check if passed id is equal to producTypes id
            if (id != productTypes.Id)
            {
                return NotFound();
            }


            //  Modelstate.IsValid - validation check on the server side if model is valid.
            //   we can have meany attributes assigned to properties in out models
            // asp.net will check against those attributes 
            if (ModelState.IsValid)
            {
                // update
                _db.Update(productTypes);
                //save changes
                await _db.SaveChangesAsync();
                // return to index with product types ; using "nameof" helps prevent spelling errors
                return RedirectToAction(nameof(Index));
            }

            return View(productTypes);
        }



        /// <summary>
        /// GET Delete action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // if id is null return nor found
            if (id == null)
            {
                return NotFound();

            }
            // if Id is not null, retrieve product type from database
            var productType = await _db.ProductTypes.FindAsync(id);
            // check if producType is null, return not found
            if (productType == null)
            {
                return NotFound();

            }
            // if producType is not null return vieW passing productType to the delete view
            return View(productType);
        }

        //POST Delete action method


        // build-in asp.net functionality
        // With each request of httpPost AntiForgeryToken is added and passes along with the request
        // Once it reaches the server it is checked if its valid (not be altered)
        /// <summary>
        /// Delete POST
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // all we need to delete is an Id
        {

            // getting id from database
            var productTypes = await _db.ProductTypes.FindAsync(id);
            //once we hav an id we just need to remove
            _db.ProductTypes.Remove(productTypes);
            //save changes
            await _db.SaveChangesAsync();
            // return to index with product types ; using "nameof" helps prevent spelling errors
            return RedirectToAction(nameof(Index));
        }

        // GET Details Action Method - retrieving Id which user wants to view
        /// <summary>
        /// Details Action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            // if id is null return nor found
            if (id == null)
            {
                return NotFound();

            }
            // if Id is not null, retrieve product type from database
            var productType = await _db.ProductTypes.FindAsync(id);
            // check if productType is null, return not found
            if (productType == null)
            {
                return NotFound();

            }

            return View(productType);

        }


    }
        
}

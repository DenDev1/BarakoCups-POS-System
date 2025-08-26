using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarakoCups.Data;
using BarakoCups.Models.Account;
using BarakoCups.Service;
using Microsoft.AspNetCore.Authorization;

namespace BarakoCups.Controllers.UserRole
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly BarakoCupsContext _context;

        public UsersController(BarakoCupsContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var barakoCupsContext = _context.Users.Include(u => u.Roles);
            return View(await barakoCupsContext.ToListAsync());
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var roles = _context.Role.ToList();
            ViewBag.RoleName = new SelectList(_context.Role, "RoleId", "RoleName");
            return PartialView("_CreateUserPartial", new Users());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FirstName,LastName,Email,Username,Password,RoleId")] Users users)
        {
            if (ModelState.IsValid)
            {
                // Hash password
                users.Password = HashingServices.HashData(users.Password);

                _context.Add(users);
                await _context.SaveChangesAsync();

                // Return JSON to tell JS to redirect to Index
                return Json(new { success = true, redirectToUrl = Url.Action("Index") });
            }

            // If validation fails, return the partial again
            ViewBag.RoleName = new SelectList(_context.Role, "RoleId", "RoleName", users.RoleId);
            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName", users.RoleId);
            return PartialView("_EditUserPartial", users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FirstName,LastName,Email,Username,Password,RoleId,ResetToken,ResetTokenExpiry")] Users users)
        {
            if (id != users.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the password before saving
                    string hashedPassword = HashingServices.HashData(users.Password);
                    users.Password = hashedPassword;
                    _context.Update(users);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName", users.RoleId);
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users != null)
            {
                _context.Users.Remove(users);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using RepositoryLayer.Entity;

//namespace RepositoryLayer.Service
//{
//    public class AddressBookRL
//    {
//        private readonly ApplicationDbContext _context;
//        public AddressBookRL(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<AddressBookEntry>> GetAllContact()
//        {
//            return await _context.AddressBookEntries.ToListAsync();
//        }

//        public async Task<AddressBookEntry?> GetContactById(int id)
//        {
//            return await _context.AddressBookEntries.FindAsync(id);
//        }

//        public async Task AddNewContact(AddressBookEntry entry)
//        {
//            await _context.AddressBookEntries.AddAsync(entry);
//            await _context.SaveChangesAsync();
//        }

//        public async Task UpdateContact(AddressBookEntry entry)
//        {
//            _context.AddressBookEntries.Update(entry);
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteContactById(int id)
//        {
//            var entry = await _context.AddressBookEntries.FindAsync(id);
//            if (entry != null)
//            {
//                _context.AddressBookEntries.Remove(entry);
//                await _context.SaveChangesAsync();
//            }
//        }


//        public async Task<List<AddressBookEntry>> GetAddressBookEntries(int id)
//        {
//            return await _context.AddressBookEntries
//           .Where(entry => entry.UserId == id)
//           .ToListAsync();
//        }
//    }




//}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;
using AddressBook.Cache;  // Import caching service

namespace RepositoryLayer.Service
{
    public class AddressBookRL
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public AddressBookRL(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<List<AddressBookEntry>> GetAllContact()
        {
            string cacheKey = "AllContacts";

            var cachedData = _cacheService.GetData<List<AddressBookEntry>>(cacheKey);
            if (cachedData != null) return cachedData;

            var contacts = await _context.AddressBookEntries.ToListAsync();
            if (contacts != null && contacts.Count > 0)
            {
                _cacheService.SetData(cacheKey, contacts, DateTimeOffset.Now.AddMinutes(10));
            }

            return contacts;
        }

        public async Task<AddressBookEntry?> GetContactById(int id)
        {
            string cacheKey = $"Contact:{id}";
            var cachedContact = _cacheService.GetData<AddressBookEntry>(cacheKey);

            if (cachedContact != null) return cachedContact;

            var contact = await _context.AddressBookEntries.FindAsync(id);
            if (contact != null)
            {
                _cacheService.SetData(cacheKey, contact, DateTimeOffset.Now.AddMinutes(10));
            }

            return contact;
        }

        public async Task AddNewContact(AddressBookEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            await _context.AddressBookEntries.AddAsync(entry);
            await _context.SaveChangesAsync();

            _cacheService.RemoveData("AllContacts"); // Invalidate all contacts cache
        }

        public async Task UpdateContact(AddressBookEntry entry)
        {
            if (entry == null || entry.Id <= 0)
                throw new ArgumentException("Invalid entry data.");

            _context.AddressBookEntries.Update(entry);
            await _context.SaveChangesAsync();

            string cacheKey = $"Contact:{entry.Id}";
            _cacheService.RemoveData(cacheKey); // Invalidate specific contact cache
            _cacheService.RemoveData("AllContacts"); // Invalidate all contacts cache
        }

        public async Task DeleteContactById(int id)
        {
            var entry = await _context.AddressBookEntries.FindAsync(id);
            if (entry != null)
            {
                _context.AddressBookEntries.Remove(entry);
                await _context.SaveChangesAsync();

                string cacheKey = $"Contact:{id}";
                _cacheService.RemoveData(cacheKey); // Invalidate deleted contact
                _cacheService.RemoveData("AllContacts"); // Invalidate all contacts cache
            }
        }

        public async Task<List<AddressBookEntry>> GetAddressBookEntries(int userId)
        {
            string cacheKey = $"UserContacts:{userId}";

            var cachedData = _cacheService.GetData<List<AddressBookEntry>>(cacheKey);
            if (cachedData != null) return cachedData;

            var contacts = await _context.AddressBookEntries
                .Where(entry => entry.UserId == userId)
                .ToListAsync();

            if (contacts != null && contacts.Count > 0)
            {
                _cacheService.SetData(cacheKey, contacts, DateTimeOffset.Now.AddMinutes(10));
            }

            return contacts;
        }
    }
}



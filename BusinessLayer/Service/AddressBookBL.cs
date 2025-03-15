using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepositoryLayer.Entity;
using RepositoryLayer.Service;

namespace BusinessLayer.Service
{
    public class AddressBookBL
    {
        private readonly AddressBookRL _addressBookRL;
        public AddressBookBL(AddressBookRL addressBookRL) 
        {
            _addressBookRL = addressBookRL;
        }

        public async Task<List<AddressBookEntry>> GetAllContactsAsync()
        {
            return await _addressBookRL.GetAllContact();
        }

        public async Task<AddressBookEntry?> GetContactByIdAsync(int id)
        {
            return await _addressBookRL.GetContactById(id);
        }

        public async Task AddNewContactAsync(AddressBookEntry entry)
        {
            await _addressBookRL.AddNewContact(entry);
        }

        public async Task UpdateContactAsync(AddressBookEntry entry)
        {
            await _addressBookRL.UpdateContact(entry);
        }

        public async Task DeleteContactByIdAsync(int id)
        {
            await _addressBookRL.DeleteContactById(id);
        }


        public async Task<List<AddressBookEntry>> GetAddressBookEntries(int id)
        {
            return await _addressBookRL.GetAddressBookEntries(id);
        }


    }
}

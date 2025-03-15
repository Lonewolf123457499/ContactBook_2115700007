using AutoMapper;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTO;
using RepositoryLayer.Entity;

[ApiController]
[Route("api/addressbook")]
public class AddressBookController : ControllerBase
{
    private readonly AddressBookBL _service;
    private readonly IMapper _mapper;

    public AddressBookController(AddressBookBL service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

       [HttpGet]
        public async Task<IActionResult> GetAllContacts()
        {
            var contacts = await _service.GetAllContactsAsync();
            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(int id)
        {
            var contact = await _service.GetContactByIdAsync(id);
            if (contact == null) return NotFound();
            return Ok(contact);
        }

    //[HttpPost]
    //public async Task<IActionResult> AddContact([FromBody] AddressBookEntryDTO entryDto)
    //{
    //    var entry = _mapper.Map<AddressBookEntry>(entryDto); // Map DTO to Entity
    //    await _service.AddNewContactAsync(entry);
    //    var resultDto = _mapper.Map<AddressBookEntryDTO>(entry); // Convert back if needed
    //    return CreatedAtAction(nameof(GetContactById), new { id = entry.Id }, resultDto);
    //}
    [HttpPost]
    public async Task<IActionResult> AddContact([FromBody] AddressBookEntryDTO entryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entry = _mapper.Map<AddressBookEntry>(entryDto);
        await _service.AddNewContactAsync(entry);
        var resultDto = _mapper.Map<AddressBookEntryDTO>(entry);

        return CreatedAtAction(nameof(GetContactById), new { id = entry.Id }, resultDto);
    }



    //[HttpPut("{id}")]
    //public async Task<IActionResult> UpdateContact(int id, [FromBody] AddressBookEntryDTO entryDto)
    //{
    //    if (entryDto == null) return BadRequest("Invalid contact data.");

    //    var existingEntry = await _service.GetContactByIdAsync(id);
    //    if (existingEntry == null) return NotFound("Contact not found.");

    //    _mapper.Map(entryDto, existingEntry);

    //    await _service.UpdateContactAsync(existingEntry);
    //    return Ok("Contact updated successfully");
    //}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] AddressBookEntryDTO entryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntry = await _service.GetContactByIdAsync(id);
        if (existingEntry == null) return NotFound("Contact not found.");

        _mapper.Map(entryDto, existingEntry);

        await _service.UpdateContactAsync(existingEntry);
        return NoContent();
    }


    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            await _service.DeleteContactByIdAsync(id);
            return Ok("Contact deleted successfully");
        }

}

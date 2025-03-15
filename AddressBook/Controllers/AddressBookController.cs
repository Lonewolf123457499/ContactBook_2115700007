using AutoMapper;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelLayer.DTO;
using RepositoryLayer.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAllContacts()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized("User not authorized");

        var userRole = GetUserRole();
        if (userRole == "Admin")
        {
            var contacts = await _service.GetAllContactsAsync();
            return Ok(contacts);
        }
        else
        {
            var contacts = await _service.GetAddressBookEntries(int.Parse(userId.ToString()));
            return Ok(contacts);

        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetContactById(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized("User not authorized");

        var userRole = GetUserRole();
        var contact = await _service.GetContactByIdAsync(id);

        if (contact == null)
            return NotFound("Contact not found");

        if (userRole != "Admin" && contact.UserId != userId)
            return Forbid("You can only view your own contacts");

        return Ok(contact);
    }


    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> AddContact([FromBody] AddressBookEntryDTO entryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == null)
            return Unauthorized("User not authorized");

        var entry = _mapper.Map<AddressBookEntry>(entryDto);
        entry.UserId = userId.Value;

        await _service.AddNewContactAsync(entry);
        var resultDto = _mapper.Map<AddressBookEntryDTO>(entry);

        return CreatedAtAction(nameof(GetContactById), new { id = entry.Id }, resultDto);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> UpdateContact(int id, [FromBody] AddressBookEntryDTO entryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == null)
            return Unauthorized("User not authorized");

        var existingEntry = await _service.GetContactByIdAsync(id);
        if (existingEntry == null)
            return NotFound("Contact not found.");

        if (existingEntry.UserId != userId)
            return Forbid("You can only update your own contacts.");

        _mapper.Map(entryDto, existingEntry);
        await _service.UpdateContactAsync(existingEntry);

        return NoContent();
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized("User not authorized");

        var userRole = GetUserRole();
        var contact = await _service.GetContactByIdAsync(id);

        if (contact == null)
            return NotFound("Contact not found");

        if (userRole != "Admin" && contact.UserId != userId)
            return Forbid("You can only delete your own contacts.");

        await _service.DeleteContactByIdAsync(id);
        return Ok("Contact deleted successfully.");
    }


    //[HttpGet("user/{userId}")]
    //[Authorize(Roles = "Admin")]
    //public async Task<IActionResult> GetContactsByUserId(int userId)
    //{
    //    var contacts = await _service.GetContactsByUserIdAsync(userId);

    //    if (contacts == null || !contacts.Any())
    //        return NotFound("No contacts found for this user");

    //    return Ok(contacts);
    //}


    private int? GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
    }


    private string GetUserRole()
    {
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        return roleClaim?.Value ?? string.Empty;
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserPortal.Data;
using UserPortal.Dtos;
using UserPortal.Entities;
using UserPortal.Services.AsyncDataServices.Interfaces;
using UserPortal.Services.Interfaces;

namespace UserPortal.Controllers
{
    public class AccountController : ApiController
    {
        private readonly IMapper _mapper;
        public AccountController(
            DataContext context, 
            ITokenService token,
            IMapper mapper,
            IMessageBusClient messageBusClient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _messageBusClient = messageBusClient ?? throw new ArgumentNullException(nameof(messageBusClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));   
        }

        public DataContext _context { get; set; }
        public ITokenService _token { get; set; }

        private readonly IMessageBusClient _messageBusClient;

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto req)
        {
            Console.WriteLine("--> Register user...");

            if (await IsUserExists(req.Username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            var user = new User()
            {
                UserName = req.Username.ToLower(),
                Email = req.Email.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(req.Password)),
                PasswordSalt = hmac.Key,
                FullName = req.FullName,
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            try
            {
                var userCreatedDto = _mapper.Map<UserCreatedDto>(user);
                userCreatedDto.Event = "User_Created";
                _messageBusClient.PublishNewUser(userCreatedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
            }

            return Ok(new UserDto
            {
                Username = user.UserName
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto req)
        {
            Console.WriteLine("--> Login user...");

            var user = await _context.Users
                .SingleOrDefaultAsync(x => x.UserName == req.Username);

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(req.Password));

            for (int i = 0; i < computedHash[i]; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid user or password");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Your account is deactive. Please contact with administrator.");
            }

            return Ok(new UserDto
            {
                Username = user.UserName,
                Token = _token.CreateToken(user)
            });
        }

        private async Task<bool> IsUserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}

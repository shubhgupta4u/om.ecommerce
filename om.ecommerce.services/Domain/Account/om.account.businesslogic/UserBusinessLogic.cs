using om.account.businesslogic.Interfaces;
using om.account.repository.Interfaces;
using om.account.model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace om.account.businesslogic
{
    public class UserBusinessLogic: IUserBusinessLogic
    {
        protected readonly IUserRepository userRepository;
        protected readonly IUserCredentialRepository credentialRepository;
        public UserBusinessLogic(IUserRepository userRepository, IUserCredentialRepository credentialRepository)
        {
            this.userRepository = userRepository;
            this.credentialRepository = credentialRepository;
        }

        public async Task<User> Create(CreateUserRequest obj)
        {
            User user = new User
            {
                Email = obj.Email,
                IsEmailVerfied = false,
                IsMobileVerfied = false,
                UserName = obj.Name,
                Mobile = obj.Mobile,
                CreateDate = System.DateTimeOffset.UtcNow
            };
            await this.userRepository.Create(user);
            UserCredential credential = new UserCredential()
            {
                UserId = user.UserId,
                Password = obj.Password,
                ExpiryDate = System.DateTimeOffset.UtcNow.AddMonths(1),
                IsDeleted = false,
                IsLocked = false,
                IsResetPwdOnFirstLogin = false
            };
            await this.credentialRepository.Create(credential);
            return user;
        }

        public Task Delete(string id)
        {
            return this.userRepository.Delete(id);
        }

        public Task<User> Get(string id)
        {
            return this.userRepository.Get(id);
        }

        public Task<IEnumerable<User>> Get()
        {
            return this.userRepository.Get();
        }

        public Task<User> GetByEmail(string email)
        {
            return this.userRepository.GetByEmail(email);
        }

        public Task<User> GetByMobile(string mobile)
        {
            return this.userRepository.GetByMobile(mobile);
        }

        public Task SetEmailVerified(string id)
        {
            return this.userRepository.SetEmailVerified(id);
        }

        public Task SetMobileVerified(string id)
        {
            return this.userRepository.SetMobileVerified(id);
        }

        public async Task<User> Update(string userId, UpdateUserRequest obj)
        {
            User oldUser = await this.userRepository.Get(userId);
            User user = new User
            {
                UserId = oldUser.UserId,
                Email = obj.Email,
                IsEmailVerfied = oldUser.Email.Equals(obj.Email,System.StringComparison.InvariantCultureIgnoreCase)? oldUser.IsEmailVerfied :false,
                IsMobileVerfied = oldUser.Mobile.Equals(obj.Mobile, System.StringComparison.InvariantCultureIgnoreCase) ? oldUser.IsMobileVerfied : false,
                UserName = obj.Name,
                Mobile = obj.Mobile
            };
            await this.userRepository.Update(user);
            return user;
        }

        public async Task<ValidateCredentialResponse> ValidateCredential(ValidateCredentialRequest request)
        {
            ValidateCredentialResponse response = new ValidateCredentialResponse();
            User user = await this.userRepository.GetByEmail(request.UserName);
            if(user != null)
            {
                response.User = user;
                UserCredential userCredential = await this.credentialRepository.GetByUserId(user.UserId);
                if (userCredential != null && userCredential.Password == request.Password)
                {
                    if (userCredential.ExpiryDate < System.DateTimeOffset.UtcNow || userCredential.IsDeleted || userCredential.IsLocked)
                    {
                        response.ErrorCode = 401;
                    }
                    response.ErrorCode = 0;
                }
                else
                {
                    response.ErrorCode = 403;
                }
            }
            else
            {
                response.ErrorCode = 403;
            }
            return response;
        }
    }
}

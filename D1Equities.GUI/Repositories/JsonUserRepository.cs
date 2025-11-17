using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace D1Equities.GUI.Model
{
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _path = "users.json";
        private List<UserModel> _users;

        public JsonUserRepository()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                _users = JsonSerializer.Deserialize<List<UserModel>>(json)
                         ?? new List<UserModel>();
            }
            else
            {
                _users = new List<UserModel>();
                Save();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        // ---- AUTH ----
        public bool AuthenticateUser(NetworkCredential credential)
        {
            var username = credential.UserName;
            var password = credential.Password;

            return _users.Any(u =>
                u.Username == username &&
                u.Password == password
            );
        }

        // ---- CRUD ----
        public void Add(UserModel userModel)
        {
            _users.Add(userModel);
            Save();
        }

        public void Edit(UserModel userModel)
        {
            var existing = _users.FirstOrDefault(u => u.Id == userModel.Id);
            if (existing == null) return;

            existing.Username = userModel.Username;
            existing.Password = userModel.Password;
            existing.Name = userModel.Name;
            existing.LastName = userModel.LastName;
            existing.Email = userModel.Email;

            Save();
        }

        public void Remove(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id.ToString());
            if (user == null) return;

            _users.Remove(user);
            Save();
        }

        public UserModel GetById(int id)
            => _users.FirstOrDefault(u => u.Id == id.ToString());

        public UserModel GetByUsername(string username)
            => _users.FirstOrDefault(u => u.Username == username);

        public IEnumerable<UserModel> GetAll() => _users;
    }
}

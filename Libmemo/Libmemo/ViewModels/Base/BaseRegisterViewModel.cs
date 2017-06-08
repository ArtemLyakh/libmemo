using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Libmemo {
    public abstract class BaseRegisterViewModel : INotifyPropertyChanged {

        private string _email;
        public string Email {
            get { return this._email; }
            set {
                if (this._email != value) {
                    this._email = value;
                    this.OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _password;
        public string Password {
            get { return this._password; }
            set {
                if (this._password != value) {
                    this._password = value;
                    this.OnPropertyChanged(nameof(Password));
                }
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword {
            get { return this._confirmPassword; }
            set {
                if (this._confirmPassword != value) {
                    this._confirmPassword = value;
                    this.OnPropertyChanged(nameof(ConfirmPassword));
                }
            }
        }

        protected virtual IEnumerable<string> Validate() {
            if (string.IsNullOrWhiteSpace(this.Email) || !Regex.IsMatch(this.Email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase)) {
                yield return "Невалидный email адрес";
            }

            if (string.IsNullOrWhiteSpace(this.Password)) {
                yield return "Не заполнен пароль";
            }
            if (string.IsNullOrWhiteSpace(this.ConfirmPassword)) {
                yield return "Не заполнено подтверждение пароля";
            }
            if (!string.Equals(this.Password, this.ConfirmPassword)) {
                yield return "Пароли не совпадают";
            }
        }








        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

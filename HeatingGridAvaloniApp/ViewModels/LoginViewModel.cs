using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia;
using System.Dynamic;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public LoginViewModel() 
        {

        }
        private string nameString = "test";

        public string NameString 
        {
            get => nameString;
            set => this.RaiseAndSetIfChanged(ref nameString, value);
        }

        public void NameButton()
        {
            NameString = "Hello!";
        }
    }


}
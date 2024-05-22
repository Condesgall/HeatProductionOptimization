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
        public string ImagePath
        {
            get => "/Assets/heater.jpg";
        }
    }
}
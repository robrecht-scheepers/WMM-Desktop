using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WheresMyMoneyApp
{
    [Activity(Label = "UserSettingsActivity")]
    public class UserSettingsActivity : Activity
    {
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.user_settings_layout);

            var settingsFragment = new UserSettingsFragment();
            var ft = FragmentManager.BeginTransaction();
            ft.Add(Resource.Id.fragment_container, settingsFragment);
            ft.Commit();
        }
    }
}
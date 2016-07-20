using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paranoid
{
	public static class RegisterServer
	{
		public static bool NewRegistration()
		{
			RegisterNewServerForm RNSF=new RegisterNewServerForm();
			return RNSF.ShowDialog() == DialogResult.OK;
		}


		public static bool CheckRegistration()
		{
			CheckServerRegistrationForm CSRF=new CheckServerRegistrationForm();
			return CSRF.ShowDialog() == DialogResult.OK;
		}
	}
}

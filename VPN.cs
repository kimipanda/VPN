using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DotRas;
using System.Windows.Forms;

namespace postwrite
{
    public class VPN
    {
        private const string EntryName = "VPN Connection";

        private RasHandle handle = null;
        private RasPhoneBook AllUsersPhoneBook;
        public RasDialer Dialer;

        // VPN 상태
        public enum status { Defalut, Connect, Error, TimeOut }
        public int VPN_Status { get; set; }

        public void VPN_Connect(string VPN_Name, string VPN_ID, string VPN_PW)
        {
            AllUsersPhoneBook = new RasPhoneBook();
            AllUsersPhoneBook.Open();

            if (AllUsersPhoneBook.Entries.Contains(EntryName))
            {
                AllUsersPhoneBook.Entries[EntryName].PhoneNumber = VPN_Name;
                AllUsersPhoneBook.Entries[EntryName].Update();

            }
            else {
                RasEntry entry = RasEntry.CreateVpnEntry(EntryName, VPN_Name, RasVpnStrategy.Default,
                                RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn));

                entry.EncryptionType = RasEncryptionType.None;

                AllUsersPhoneBook.Entries.Add(entry);
            }

            Dialer = new RasDialer();
            Dialer.DialCompleted += new EventHandler<DialCompletedEventArgs>(Dialer_DialCompleted);

            this.Dialer.EntryName = EntryName;
            this.Dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);

            try
            {
                this.Dialer.Credentials = new NetworkCredential(VPN_ID, VPN_PW);
                this.handle = this.Dialer.DialAsync();
                VPN_Status = (int)status.Defalut;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void VPN_DisConnect()
        {
            if (handle == null) return;

            if (this.Dialer.IsBusy)
            {
                this.Dialer.DialAsyncCancel();
            }
            else
            {
                RasConnection connection = RasConnection.GetActiveConnectionByHandle(this.handle);
                if (connection != null)
                {
                    // The connection has been found, disconnect it.
                    connection.HangUp();
                    this.VPN_Status = (int)status.Defalut;
                }
            }
        }

        public void Dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                VPN_Status = (int)status.Error;
                MessageBox.Show("Cancelled");
            }
            else if (e.TimedOut)
            {
                VPN_Status = (int)status.TimeOut;
                MessageBox.Show("timeout");
            }
            else if (e.Error != null)
            {
                VPN_Status = (int)status.Error;
                MessageBox.Show(e.Error.ToString());
            }
            else if (e.Connected)
            {
                VPN_Status = (int)status.Connect;
                MessageBox.Show("연결성공");
            }

            if (!e.Connected)
            {
                VPN_Status = (int)status.Error;
                MessageBox.Show("!e Connected");
            }
        }
    }
}

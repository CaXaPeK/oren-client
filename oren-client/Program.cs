using oren_client.Lib.Formats;

/*if (args.Length != 1)
{
    return;
}

string fileName = args[0];*/

byte[] msbtBytes = File.ReadAllBytes("C:\\Users\\student\\RiderProjects\\oren-client\\oren-client\\bin\\Debug\\net7.0\\base_mfre_101.msbt");

MSBT msbt = new(msbtBytes);

msbt.PrintAllMessages();
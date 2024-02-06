using System;
using Google.Cloud.Firestore;

namespace apiBillFold.Externo
{
    [FirestoreData]
    public class users
	{
		public users()
		{
		}
        [FirestoreProperty]
        public DocumentReference fcm_tokens { get; set; }
        [FirestoreProperty]
        public object display_time { get; set; }
        [FirestoreProperty]
        public string dpi { get; set; }
        [FirestoreProperty]
        public string email { get; set; }
        [FirestoreProperty]
        public string nit { get; set; }
        [FirestoreProperty]
        public string photo_url { get; set; }
        [FirestoreProperty]
        public double saldo { get; set; }
        [FirestoreProperty]
        public string uid { get; set; }
        [FirestoreProperty]
        public int visita { get; set; }
    }
}




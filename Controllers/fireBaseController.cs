using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apiBillFold.Externo;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Dac.Model;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Type;
using apiBillFold.Models;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace apiBillFold.Controllers
{
    [Route("api/[controller]")]
    public class fireBaseController : Controller
    {
        // GET: api/values
        private FirestoreDb db;
        private FirebaseApp app;

        public fireBaseController()
        {
            try
            {
                var jsonString = System.IO.File.ReadAllText("billeteracm-firebase-adminsdk-pqt8z-ea5393d5f7.json");
                var builder = new FirestoreClientBuilder { JsonCredentials = jsonString };
                db = FirestoreDb.Create("billeteracm", builder.Build());
                // Construct the message payload
                app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("billeteracm-firebase-adminsdk-pqt8z-ea5393d5f7.json"),
                });
            }
            catch (Exception e)
            {


            }
        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            categoria datos = new categoria();
            datos.imagen = "";
            datos.Categoria = "Reposteria";
            db.Collection("Categorias").AddAsync(
                datos
                );
        }

        [HttpPost("agregarTransaccion")]
        public async void agregarTransaccion(string categoria, string monto, string nombreOperacion, string lugar, string razon, string factura, string usuario)
        {
            transaccion datos = new transaccion();
            datos.categoria = categoria;
            datos.transactionAmount = double.Parse(monto);
            datos.transactionName = nombreOperacion;
            datos.transactionPlace = lugar;
            datos.transactionReason = razon;
            datos.transactionTime = FieldValue.ServerTimestamp;
            datos.NoFactura = int.Parse(factura);
            datos.user = db.Document("users/" + usuario);
            var respuesta = db.Collection("transactions").AddAsync(
                datos
                );
            DocumentReference refUser = db.Collection("users").Document(usuario);
            DocumentSnapshot actual = await refUser.GetSnapshotAsync();
            Dictionary<string, object> saldoActual = actual.ToDictionary();
            double saldo = double.Parse(saldoActual["saldo"].ToString());
            double final = 0;
            if (categoria == "debito")
            {
                final = saldo - Double.Parse(monto);
            }
            else
            {
                final = saldo + Double.Parse(monto);
            }
            var respuesta2 = refUser.UpdateAsync("saldo", final);
        }



        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet("obtenerCategorias")]
        public async Task<ActionResult<clsRespuesta>> getCategorias()
        {
            clsRespuesta respuesta = new clsRespuesta();
            CollectionReference categoriasRef = db.Collection("Categorias");
            QuerySnapshot snapshot = await categoriasRef.GetSnapshotAsync();
            List<categoria> ListaCategorias = new List<categoria>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                categoria nuevaCategoria = new categoria();
                nuevaCategoria.Id = document.Id;
                Dictionary<string, object> dicCategoria = document.ToDictionary();
                nuevaCategoria.Categoria = dicCategoria["Categoria"].ToString();
                nuevaCategoria.imagen = dicCategoria["imagen"].ToString();
                nuevaCategoria.tienda = ((DocumentReference)dicCategoria["Tienda"]).Id;
                ListaCategorias.Add(nuevaCategoria);
            }
            if (ListaCategorias.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se ennconntraron Categorias";
            }
            else
            {
                respuesta.resultado = true;
                respuesta.data = ListaCategorias;

            }

            return respuesta;
        }

        [HttpGet("obtenerTiendas")]
        public async Task<ActionResult<clsRespuesta>> getTiendas()
        {
            clsRespuesta respuesta = new clsRespuesta();
            CollectionReference categoriasRef = db.Collection("Tiendas");
            QuerySnapshot snapshot = await categoriasRef.GetSnapshotAsync();
            List<Externo.Tienda> ListaCategorias = new List<Externo.Tienda>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Externo.Tienda nuevaCategoria = new Externo.Tienda();
                nuevaCategoria.Id = document.Id;
                Dictionary<string, object> dicCategoria = document.ToDictionary();
                nuevaCategoria.nombre = dicCategoria["Nombre"].ToString();
                nuevaCategoria.imagen = dicCategoria["imagen"].ToString();
                ListaCategorias.Add(nuevaCategoria);
            }
            if (ListaCategorias.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se ennconntraron Categorias";
            }
            else
            {
                respuesta.resultado = true;
                respuesta.data = ListaCategorias;

            }

            return respuesta;
        }

        [HttpGet("obtenerProductos")]
        public async Task<ActionResult<clsRespuesta>> getProductos(String uidTienda)
        {
            clsRespuesta respuesta = new clsRespuesta();
            List<productos> ListaCategorias = new List<productos>();
            /*obtener categoria*/
            Query categoriasquery = db.Collection("Categorias").WhereEqualTo("Tienda", db.Document("Tiendas/" + uidTienda));
            QuerySnapshot snapshotCategorias = await categoriasquery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentoCategoria in snapshotCategorias.Documents) {
                categoria cat = new categoria();
                Dictionary<string, object> dicCategoria = documentoCategoria.ToDictionary();
                DocumentReference catReference= documentoCategoria.Reference;
                String UIDcat = documentoCategoria.Id;
                Query ProductoQuery = db.Collection("Productos").WhereEqualTo("categoria", db.Document("Categorias/"+ UIDcat));
                QuerySnapshot snapshotProd = await ProductoQuery.GetSnapshotAsync();
                foreach (DocumentSnapshot document in snapshotProd.Documents)
                {
                    productos nuevaCategoria = new productos();
                    nuevaCategoria.id = document.Id;
                    Dictionary<string, object> dicProducto = document.ToDictionary();
                    nuevaCategoria.name = dicProducto["name"].ToString();
                    nuevaCategoria.imagen = dicProducto["imagen"].ToString();
                    nuevaCategoria.descripcion = dicProducto["description"].ToString();
                    nuevaCategoria.precio = decimal.Parse(dicProducto["price"].ToString());
                    ListaCategorias.Add(nuevaCategoria);
                }

                cat.Id = "asdfsd";
            }

            /*obtener Productos*/
       //     CollectionReference ProductosREf = db.Collection("Productos");
           //// QuerySnapshot snapshot = await ProductosREf.GetSnapshotAsync();
            

           
            if (ListaCategorias.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se ennconntraron Categorias";
            }
            else
            {
                respuesta.resultado = true;
                respuesta.data = ListaCategorias;

            }

            return respuesta;
        }
        [HttpGet("crearOrden")]
        public async Task<ActionResult<clsRespuesta>> crearOrden(double amount, double numero, string status, string user, string vendor_name, List<dataOrdenes> detalle, string tienda) {


            Dictionary<string, object> orden = new Dictionary<string, object>
            {
                { "amount", amount },
                { "created_at", FieldValue.ServerTimestamp},
                { "direccion", "" },
                { "number",numero},
                { "status",status},
                { "user",user },
                { "vendor_name",vendor_name},
                { "Tienda",db.Document("Tiendas/"+tienda)}
            };
            DocumentReference addedDocRef = await db.Collection("ordenes").AddAsync(orden);
            foreach (dataOrdenes dt in detalle) {
                Dictionary<string, object> filaOrden = new Dictionary<string, object>
                {
                    { "cantidad", Double.Parse(dt.cantidad) },
                    { "nombre", dt.nombre},
                    { "precio", Double.Parse(dt.precio)},
                    { "subtotal", Double.Parse((Double.Parse(dt.precio)*Double.Parse(dt.cantidad)).ToString())},
                    { "ReferenceOrden", addedDocRef},
                    { "ReferenceProduct",db.Document("Productos/"+dt.codigo.Replace("#",""))  }
                };
                DocumentReference tempFilaRef = await db.Collection("detalleOrden").AddAsync(filaOrden);
            }
            return new clsRespuesta();
        }

        [HttpGet("obtenerOrdenes")]
        public async Task<ActionResult<clsRespuesta>> getOrdenes(String tienda, string usuario)
        {
            clsRespuesta respuesta = new clsRespuesta();
            var dataCliente = await obtenerUsrReference(usuario);

            Query categoriasRef = db.Collection("ordenes").WhereEqualTo("Tienda", db.Document("Tiendas/" + tienda)).WhereEqualTo("user", dataCliente.Value);
            QuerySnapshot snapshot = await categoriasRef.GetSnapshotAsync();
            List<Externo.Orden> ListaCategorias = new List<Externo.Orden>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Externo.Orden nuevaCategoria = new Externo.Orden();
                nuevaCategoria.id = document.Id;
                Dictionary<string, object> dicCategoria = document.ToDictionary();
                nuevaCategoria.monto = System.Decimal.Parse(dicCategoria["amount"].ToString());
                nuevaCategoria.estado = dicCategoria["status"].ToString();
                nuevaCategoria.vendedor = dicCategoria["vendor_name"].ToString();
                nuevaCategoria.numero = Int32.Parse(dicCategoria["number"].ToString());
                nuevaCategoria.direccion = dicCategoria["direccion"].ToString();
                DocumentReference docRefUser = db.Document("users/" + dicCategoria["user"].ToString());
                DocumentSnapshot documnt1 = await docRefUser.GetSnapshotAsync();
                Dictionary<string, object> dicdocumnt1 = documnt1.ToDictionary();
                nuevaCategoria.usuario = dicdocumnt1["display_name"].ToString() + " " + dicdocumnt1["email"].ToString();
                ListaCategorias.Add(nuevaCategoria);
            }
            if (ListaCategorias.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se ennconntraron Categorias";
            }
            else
            {
                respuesta.resultado = true;
                respuesta.data = ListaCategorias;

            }

            return respuesta;
        }

        [HttpGet("obtenerDetalleOrdenes")]
        public async Task<ActionResult<clsRespuesta>> getDetalleOrdenes(String idOrden)
        {
            clsRespuesta respuesta = new clsRespuesta();
            // var dataCliente = await obtenerUsrReference(usuario);

            Query categoriasRef = db.Collection("detalleOrden").WhereEqualTo("ReferenceOrden", db.Document("ordenes/" + idOrden));
             QuerySnapshot snapshot = await categoriasRef.GetSnapshotAsync();
            List<detalleOrden> ListaCategorias = new List<detalleOrden>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                detalleOrden nuevaCategoria = new detalleOrden();
                Dictionary<string, object> dicCategoria = document.ToDictionary();
                nuevaCategoria.cantidad = System.Int32.Parse(dicCategoria["cantidad"].ToString());
                nuevaCategoria.nombre = dicCategoria["nombre"].ToString();
                nuevaCategoria.precio = System.Decimal.Parse(dicCategoria["precio"].ToString());
                nuevaCategoria.subtotal = System.Decimal.Parse(dicCategoria["subtotal"].ToString());
                ListaCategorias.Add(nuevaCategoria);
            }
            if (ListaCategorias.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se ennconntraron Categorias";
            }
            else
            {
                respuesta.resultado = true;
                respuesta.data = ListaCategorias;

            }

            return respuesta;
        }


        [HttpGet("obtenerTransacciones")]
        public async Task<ActionResult<clsRespuesta>> getTransacciones()
        {
            clsRespuesta respuesta = new clsRespuesta();
            CollectionReference categoriasRef = db.Collection("transactions");
            QuerySnapshot snapshot = await categoriasRef.GetSnapshotAsync();
            List<transaccion> ListaCategorias = new List<transaccion>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                transaccion nuevaCategoria = new transaccion();
                nuevaCategoria.id = document.Id;
                Dictionary<string, object> dicCategoria = document.ToDictionary();
                nuevaCategoria.transactionName = dicCategoria["transactionName"].ToString();
                nuevaCategoria.transactionAmount = double.Parse(dicCategoria["transactionAmount"].ToString());
                nuevaCategoria.transactionPlace = dicCategoria["transactionPlace"].ToString();
                nuevaCategoria.transactionReason = dicCategoria["transactionReason"].ToString();
                nuevaCategoria.transactionTime = dicCategoria["transactionTime"].ToString();
                //        nuevaCategoria.user = dicCategoria["user"].ToString();
                nuevaCategoria.NoFactura = int.Parse(dicCategoria["NoFactura"].ToString());
                nuevaCategoria.categoria = dicCategoria["categoria"].ToString();
                ListaCategorias.Add(nuevaCategoria);
            }
            if (ListaCategorias.Count == 0)
            {
                respuesta.resultado = false;
                respuesta.mensaje = "No se ennconntraron Categorias";
            }
            else
            {
                respuesta.resultado = true;
                respuesta.data = ListaCategorias;

            }

            return respuesta;
        }

        [HttpGet("subirfichero")]
        public async Task<ActionResult<clsRespuesta>> uploadDocument()
        {
            var stream = System.IO.File.Open(@"/Users/user/Downloads/My project-1 (9).png", FileMode.Open);

            // Construct FirebaseStorage with path to where you want to upload the file and put it there
            var task = new FirebaseStorage("billeteracm.appspot.com")
             .Child("data")
             .Child("file.png")
             .PutAsync(stream);

            // Track progress of the upload
            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

            // Await the task to wait until upload is completed and get the download url
            var downloadUrl = await task;
            return new clsRespuesta();
        }
        [HttpGet("NotificarAutorizacion")]
        public async Task<ActionResult<clsRespuesta>> enviarTokenAutorizacion(String Titulo, String Body, String Token, string nToken)
        {

            var message = new Message()
            {

                Notification = new Notification
                {
                    Title = Titulo,
                    Body = Body

                },
                Data = new Dictionary<string, string>() {
                    {  "Token",nToken}
                },
                Token = Token
            };
            // Send the message
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return new clsRespuesta();
        }

        [HttpGet("enviarPushNotification")]
        public async Task<ActionResult<clsRespuesta>> enviarrNotificacion(String Titulo, String Body, String Token)
        {

            var message = new Message()
            {

                Notification = new Notification
                {
                    Title = Titulo,
                    Body = Body

                },
               
                Token = Token
            };
            // Send the message
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return new clsRespuesta();
        }
        [HttpGet("obtenerFCM")]
        public async Task<ActionResult<string>> obtenerTokenUsuario(String Correo)
        {
            CollectionReference userRef = db.Collection("users");
            Query query = userRef.WhereEqualTo("email", Correo);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            String fcnToken = "No Existe";
            foreach (DocumentSnapshot document in querySnapshot.Documents)
            {
                //Dictionary<string, object> dicCategoria = document.ToDictionary();
                CollectionReference fcmTokensGroupRef= document.Reference.Collection("fcm_tokens");
                if (fcmTokensGroupRef == null) {
                    return "No Existe";
                }
                else
                {
                    QuerySnapshot snapshotFcm = await fcmTokensGroupRef.GetSnapshotAsync();
                    foreach(DocumentSnapshot fcmDocument in snapshotFcm.Documents)
                    {
                        Dictionary<string, object> dicCategoria = fcmDocument.ToDictionary();
                        fcnToken = dicCategoria["fcm_token"].ToString();

                    }
                }
            }
            return fcnToken;
        }
        [HttpGet("obtenerRefUsr")]
        public async Task<ActionResult<string>> obtenerUsrReference(String Correo)
        {
            CollectionReference userRef = db.Collection("users");
            Query query = userRef.WhereEqualTo("email", Correo);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            String fcnToken = "No Existe";
            foreach (DocumentSnapshot document in querySnapshot.Documents)
            {
                Dictionary<string, object> dicCategoria = document.ToDictionary();
                return dicCategoria["uid"].ToString();
            }
            return fcnToken;
        }
    }
}
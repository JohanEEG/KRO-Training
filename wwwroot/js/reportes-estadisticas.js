// Reporte de pagos e ingresos (Reportes > Estadísticas)
// Datos de prueba: se reemplazarán por datos reales en el sprint 3.
(function () {

    var PAGOS = [
        { fecha: "2026-08-17", cliente: "Juan Pérez",      plan: "Mensual",    metodo: "Tarjeta",       monto: 25000 },
        { fecha: "2026-08-16", cliente: "María Rodríguez", plan: "Trimestral", metodo: "Transferencia", monto: 65000 },
        { fecha: "2026-08-15", cliente: "Carlos Vargas",   plan: "Mensual",    metodo: "Efectivo",      monto: 25000 },
        { fecha: "2026-08-14", cliente: "Ana González",    plan: "Mensual",    metodo: "Tarjeta",       monto: 25000 }
    ];

    var TIPO_DISPONIBLE = "Pagos e ingresos";
    var ARROBA = String.fromCharCode(64);

    var btnGenerar = document.getElementById("btnGenerar");
    var modal = document.getElementById("modalReporte");
    var modalIcono = document.getElementById("modalIcono");
    var modalTitulo = document.getElementById("modalTitulo");
    var modalMensaje = document.getElementById("modalMensaje");
    var modalAcciones = document.getElementById("modalAcciones");

    if (!btnGenerar || !modal) return;

    var ultimoReporte = null;

    function formatoFecha(iso) {
        var p = iso.split("-");
        return p[2] + "/" + p[1] + "/" + p[0];
    }

    function formatoMonto(n) {
        return "₡" + n.toLocaleString("es-ES");
    }

    function crearBoton(texto, clase, onClick) {
        var b = document.createElement("button");
        b.type = "button";
        b.className = "kro-btn " + clase;
        b.textContent = texto;
        b.onclick = onClick;
        return b;
    }

    function cerrarModal() {
        modal.setAttribute("hidden", "true");
    }

    function abrirModal(icono, titulo, mensaje, acciones) {
        modalIcono.textContent = icono;
        modalTitulo.textContent = titulo;
        modalMensaje.textContent = mensaje;
        modalAcciones.innerHTML = "";
        acciones.forEach(function (a) { modalAcciones.appendChild(a); });
        modal.removeAttribute("hidden");
    }

    function construirReporteHtml(r) {
        var filas = r.datos.map(function (p) {
            return "<tr>" +
                "<td>" + formatoFecha(p.fecha) + "</td>" +
                "<td>" + p.cliente + "</td>" +
                "<td>" + p.plan + "</td>" +
                "<td>" + p.metodo + "</td>" +
                "<td style='text-align:right'>" + formatoMonto(p.monto) + "</td>" +
                "</tr>";
        }).join("");

        var total = r.datos.reduce(function (s, p) { return s + p.monto; }, 0);

        return "<!DOCTYPE html><html lang='es'><head><meta charset='utf-8'>" +
            "<title>Reporte de pagos e ingresos - KRÒ Training (prueba)</title>" +
            "<link href='https://fonts.googleapis.com/css2?family=Montserrat:wght" + ARROBA + "600;700&family=Roboto:wght" + ARROBA + "400;500&display=swap' rel='stylesheet'>" +
            "<style>" +
            "body{font-family:Roboto,sans-serif;color:#1C1C1C;margin:0;background:#F4F4F4;}" +
            ".hoja{max-width:820px;margin:30px auto;background:#fff;box-shadow:0 4px 18px rgba(0,0,0,.12);}" +
            ".cab{background:#1C1C1C;color:#fff;padding:26px 34px;border-bottom:4px solid #D62828;display:flex;justify-content:space-between;align-items:center;}" +
            ".cab h1{font-family:Montserrat,sans-serif;margin:0;font-size:26px;}" +
            ".cab h1 span{color:#D62828;}" +
            ".cab small{color:#BBBBBB;letter-spacing:3px;}" +
            ".cuerpo{padding:30px 34px;}" +
            ".titulo{font-family:Montserrat,sans-serif;font-size:20px;font-weight:600;margin:0 0 6px;}" +
            ".meta{color:#666;font-size:14px;margin-bottom:24px;}" +
            "table{width:100%;border-collapse:collapse;font-size:14px;}" +
            "thead{background:#1C1C1C;color:#fff;}" +
            "th{padding:11px;text-align:left;font-family:Montserrat,sans-serif;font-size:13px;}" +
            "td{padding:11px;border-bottom:1px solid #DDD;}" +
            ".total{margin-top:20px;text-align:right;font-family:Montserrat,sans-serif;font-size:18px;}" +
            ".total span{color:#D62828;}" +
            ".aviso{margin-top:34px;padding:14px 16px;border-left:4px solid #D62828;background:#FDE8E8;font-size:13px;color:#1C1C1C;}" +
            ".acciones{text-align:center;padding:20px;}" +
            "button{background:#2563EB;color:#fff;border:none;border-radius:6px;padding:11px 20px;font-family:Roboto,sans-serif;font-size:14px;cursor:pointer;}" +
            "</style></head><body>" +
            "<div class='hoja'>" +
            "<div class='cab'><h1>KR<span>Ò</span> Training</h1><small>REPORTE</small></div>" +
            "<div class='cuerpo'>" +
            "<div class='titulo'>" + r.tipo + "</div>" +
            "<div class='meta'>Periodo: " + formatoFecha(r.ini) + " al " + formatoFecha(r.fin) + " &nbsp;·&nbsp; Registros: " + r.datos.length + "</div>" +
            "<table><thead><tr><th>Fecha</th><th>Cliente</th><th>Plan</th><th>Método de pago</th><th style='text-align:right'>Monto</th></tr></thead>" +
            "<tbody>" + filas + "</tbody></table>" +
            "<div class='total'>Total del periodo: <span>" + formatoMonto(total) + "</span></div>" +
            "<div class='aviso'><strong>Documento de prueba.</strong> Los datos son ficticios y no representan transacciones bancarias ni comprobantes de pago reales.</div>" +
            "</div>" +
            "<div class='acciones'><button onclick='window.print()'>Imprimir / Guardar como PDF</button></div>" +
            "</div></body></html>";
    }

    function verReporte() {
        if (!ultimoReporte) return;
        var ventana = window.open("", "_blank");
        if (!ventana) {
            alert("El navegador bloqueó la ventana del reporte. Permita las ventanas emergentes e intente de nuevo.");
            return;
        }
        ventana.document.open();
        ventana.document.write(construirReporteHtml(ultimoReporte));
        ventana.document.close();
    }

    function generarReporte() {
        var ini = document.getElementById("fechaInicial").value;
        var fin = document.getElementById("fechaFinal").value;
        var tipo = document.getElementById("tipoEstadistica").value;

        btnGenerar.disabled = true;
        btnGenerar.textContent = "Generando...";

        setTimeout(function () {
            btnGenerar.disabled = false;
            btnGenerar.textContent = "Generar reporte";

            if (tipo !== TIPO_DISPONIBLE) {
                abrirModal("🛠️", "Próximamente",
                    "El reporte de " + tipo.toLowerCase() + " estará disponible en el sprint 3.",
                    [crearBoton("Aceptar", "kro-btn-primary", cerrarModal)]);
                return;
            }

            if (!ini || !fin || ini > fin) {
                abrirModal("⚠️", "Error al generar el reporte",
                    "No fue posible generar el reporte. Revise el periodo seleccionado e intente de nuevo.",
                    [crearBoton("Aceptar", "kro-btn-primary", cerrarModal)]);
                return;
            }

            var datos = PAGOS.filter(function (p) {
                return p.fecha >= ini && p.fecha <= fin;
            });

            if (datos.length === 0) {
                abrirModal("📭", "Sin datos en el periodo seleccionado",
                    "No hay pagos registrados entre " + formatoFecha(ini) + " y " + formatoFecha(fin) + ". Cambie el periodo e intente de nuevo.",
                    [crearBoton("Aceptar", "kro-btn-primary", cerrarModal)]);
                return;
            }

            ultimoReporte = { ini: ini, fin: fin, tipo: tipo, datos: datos };

            abrirModal("✅", "Reporte creado correctamente",
                "Se encontraron " + datos.length + " pagos entre " + formatoFecha(ini) + " y " + formatoFecha(fin) + ".",
                [
                    crearBoton("Ver reporte PDF", "kro-btn-primary", verReporte),
                    crearBoton("Cerrar", "kro-btn-outline", cerrarModal)
                ]);
        }, 1200);
    }

    btnGenerar.addEventListener("click", generarReporte);

    modal.addEventListener("click", function (e) {
        if (e.target === modal) cerrarModal();
    });

})();

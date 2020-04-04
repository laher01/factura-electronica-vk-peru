namespace Datos
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSalir = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.txServidor = new System.Windows.Forms.TextBox();
            this.txServidorLic = new System.Windows.Forms.TextBox();
            this.txUsuarioSAP = new System.Windows.Forms.TextBox();
            this.txPassSAP = new System.Windows.Forms.TextBox();
            this.txVersionSQL = new System.Windows.Forms.TextBox();
            this.txUsuarioBase = new System.Windows.Forms.TextBox();
            this.txPassBase = new System.Windows.Forms.TextBox();
            this.txSistemaSAP = new System.Windows.Forms.TextBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.cbxOPGetEstado = new System.Windows.Forms.ComboBox();
            this.txOPGetEstado = new System.Windows.Forms.TextBox();
            this.txMailEnvia = new System.Windows.Forms.TextBox();
            this.txMailSmtp = new System.Windows.Forms.TextBox();
            this.txMailUsuario = new System.Windows.Forms.TextBox();
            this.txMailPassword = new System.Windows.Forms.TextBox();
            this.txMailPuerto = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.cbxMailEnvio = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.txHoraEnvio1 = new System.Windows.Forms.TextBox();
            this.txHoraEnvio2 = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.lbxBases = new System.Windows.Forms.ListBox();
            this.txAgregarBase = new System.Windows.Forms.TextBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnBorrar = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSalir
            // 
            this.btnSalir.Location = new System.Drawing.Point(486, 421);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(75, 23);
            this.btnSalir.TabIndex = 0;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Servidor";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Servidor Licencia";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(41, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Usuario SAP";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Password SAP";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(41, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Versión SQL";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 146);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Usuario Base";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(41, 172);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Password Base";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(41, 198);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Sistema SAP";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(41, 224);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "OP GetEstado";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(41, 259);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(88, 13);
            this.label16.TabIndex = 16;
            this.label16.Text = "Correo que envia";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(41, 285);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(87, 13);
            this.label17.TabIndex = 17;
            this.label17.Text = "Correo SmtpHost";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(41, 311);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(77, 13);
            this.label18.TabIndex = 18;
            this.label18.Text = "Correo Usuario";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(41, 337);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(87, 13);
            this.label19.TabIndex = 19;
            this.label19.Text = "Correo Password";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(41, 363);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(72, 13);
            this.label20.TabIndex = 20;
            this.label20.Text = "Correo Puerto";
            // 
            // txServidor
            // 
            this.txServidor.Location = new System.Drawing.Point(138, 14);
            this.txServidor.Name = "txServidor";
            this.txServidor.Size = new System.Drawing.Size(132, 20);
            this.txServidor.TabIndex = 21;
            // 
            // txServidorLic
            // 
            this.txServidorLic.Location = new System.Drawing.Point(138, 39);
            this.txServidorLic.Name = "txServidorLic";
            this.txServidorLic.Size = new System.Drawing.Size(132, 20);
            this.txServidorLic.TabIndex = 22;
            // 
            // txUsuarioSAP
            // 
            this.txUsuarioSAP.Location = new System.Drawing.Point(138, 65);
            this.txUsuarioSAP.Name = "txUsuarioSAP";
            this.txUsuarioSAP.Size = new System.Drawing.Size(132, 20);
            this.txUsuarioSAP.TabIndex = 23;
            // 
            // txPassSAP
            // 
            this.txPassSAP.Location = new System.Drawing.Point(138, 91);
            this.txPassSAP.Name = "txPassSAP";
            this.txPassSAP.PasswordChar = '*';
            this.txPassSAP.Size = new System.Drawing.Size(132, 20);
            this.txPassSAP.TabIndex = 24;
            // 
            // txVersionSQL
            // 
            this.txVersionSQL.Location = new System.Drawing.Point(138, 117);
            this.txVersionSQL.Name = "txVersionSQL";
            this.txVersionSQL.Size = new System.Drawing.Size(132, 20);
            this.txVersionSQL.TabIndex = 25;
            // 
            // txUsuarioBase
            // 
            this.txUsuarioBase.Location = new System.Drawing.Point(138, 143);
            this.txUsuarioBase.Name = "txUsuarioBase";
            this.txUsuarioBase.Size = new System.Drawing.Size(132, 20);
            this.txUsuarioBase.TabIndex = 26;
            // 
            // txPassBase
            // 
            this.txPassBase.Location = new System.Drawing.Point(138, 169);
            this.txPassBase.Name = "txPassBase";
            this.txPassBase.PasswordChar = '*';
            this.txPassBase.Size = new System.Drawing.Size(132, 20);
            this.txPassBase.TabIndex = 27;
            // 
            // txSistemaSAP
            // 
            this.txSistemaSAP.Location = new System.Drawing.Point(138, 195);
            this.txSistemaSAP.Name = "txSistemaSAP";
            this.txSistemaSAP.Size = new System.Drawing.Size(132, 20);
            this.txSistemaSAP.TabIndex = 28;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Enabled = false;
            this.btnGuardar.Location = new System.Drawing.Point(486, 381);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(75, 23);
            this.btnGuardar.TabIndex = 41;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // cbxOPGetEstado
            // 
            this.cbxOPGetEstado.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxOPGetEstado.FormattingEnabled = true;
            this.cbxOPGetEstado.Items.AddRange(new object[] {
            "Si",
            "No"});
            this.cbxOPGetEstado.Location = new System.Drawing.Point(128, 221);
            this.cbxOPGetEstado.Name = "cbxOPGetEstado";
            this.cbxOPGetEstado.Size = new System.Drawing.Size(40, 21);
            this.cbxOPGetEstado.TabIndex = 42;
            // 
            // txOPGetEstado
            // 
            this.txOPGetEstado.Location = new System.Drawing.Point(190, 221);
            this.txOPGetEstado.Name = "txOPGetEstado";
            this.txOPGetEstado.Size = new System.Drawing.Size(360, 20);
            this.txOPGetEstado.TabIndex = 43;
            // 
            // txMailEnvia
            // 
            this.txMailEnvia.Location = new System.Drawing.Point(138, 256);
            this.txMailEnvia.Name = "txMailEnvia";
            this.txMailEnvia.Size = new System.Drawing.Size(316, 20);
            this.txMailEnvia.TabIndex = 56;
            // 
            // txMailSmtp
            // 
            this.txMailSmtp.Location = new System.Drawing.Point(138, 282);
            this.txMailSmtp.Name = "txMailSmtp";
            this.txMailSmtp.Size = new System.Drawing.Size(316, 20);
            this.txMailSmtp.TabIndex = 57;
            // 
            // txMailUsuario
            // 
            this.txMailUsuario.Location = new System.Drawing.Point(138, 308);
            this.txMailUsuario.Name = "txMailUsuario";
            this.txMailUsuario.Size = new System.Drawing.Size(132, 20);
            this.txMailUsuario.TabIndex = 58;
            // 
            // txMailPassword
            // 
            this.txMailPassword.Location = new System.Drawing.Point(138, 334);
            this.txMailPassword.Name = "txMailPassword";
            this.txMailPassword.PasswordChar = '*';
            this.txMailPassword.Size = new System.Drawing.Size(132, 20);
            this.txMailPassword.TabIndex = 59;
            // 
            // txMailPuerto
            // 
            this.txMailPuerto.Location = new System.Drawing.Point(138, 360);
            this.txMailPuerto.Name = "txMailPuerto";
            this.txMailPuerto.Size = new System.Drawing.Size(47, 20);
            this.txMailPuerto.TabIndex = 60;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(41, 386);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(59, 13);
            this.label21.TabIndex = 61;
            this.label21.Text = "Enviar Mail";
            // 
            // cbxMailEnvio
            // 
            this.cbxMailEnvio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMailEnvio.FormattingEnabled = true;
            this.cbxMailEnvio.Items.AddRange(new object[] {
            "Si",
            "No"});
            this.cbxMailEnvio.Location = new System.Drawing.Point(138, 383);
            this.cbxMailEnvio.Name = "cbxMailEnvio";
            this.cbxMailEnvio.Size = new System.Drawing.Size(40, 21);
            this.cbxMailEnvio.TabIndex = 62;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(44, 413);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(63, 13);
            this.label22.TabIndex = 63;
            this.label22.Text = "HoraEnvio1";
            // 
            // txHoraEnvio1
            // 
            this.txHoraEnvio1.Location = new System.Drawing.Point(138, 410);
            this.txHoraEnvio1.Name = "txHoraEnvio1";
            this.txHoraEnvio1.Size = new System.Drawing.Size(47, 20);
            this.txHoraEnvio1.TabIndex = 64;
            // 
            // txHoraEnvio2
            // 
            this.txHoraEnvio2.Location = new System.Drawing.Point(138, 436);
            this.txHoraEnvio2.Name = "txHoraEnvio2";
            this.txHoraEnvio2.Size = new System.Drawing.Size(47, 20);
            this.txHoraEnvio2.TabIndex = 66;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(44, 439);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(63, 13);
            this.label23.TabIndex = 65;
            this.label23.Text = "HoraEnvio2";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(191, 413);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(85, 13);
            this.label24.TabIndex = 67;
            this.label24.Text = "Formato HH:MM";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(191, 439);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(85, 13);
            this.label25.TabIndex = 68;
            this.label25.Text = "Formato HH:MM";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(370, 421);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 23);
            this.button1.TabIndex = 69;
            this.button1.Text = "Encriptar XML";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(340, 17);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(36, 13);
            this.label26.TabIndex = 70;
            this.label26.Text = "Bases";
            // 
            // lbxBases
            // 
            this.lbxBases.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.lbxBases.FormattingEnabled = true;
            this.lbxBases.Location = new System.Drawing.Point(343, 38);
            this.lbxBases.Name = "lbxBases";
            this.lbxBases.Size = new System.Drawing.Size(160, 173);
            this.lbxBases.TabIndex = 71;
            // 
            // txAgregarBase
            // 
            this.txAgregarBase.Location = new System.Drawing.Point(395, 14);
            this.txAgregarBase.Name = "txAgregarBase";
            this.txAgregarBase.Size = new System.Drawing.Size(100, 20);
            this.txAgregarBase.TabIndex = 72;
            // 
            // btnAgregar
            // 
            this.btnAgregar.Location = new System.Drawing.Point(509, 62);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(75, 23);
            this.btnAgregar.TabIndex = 73;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // btnBorrar
            // 
            this.btnBorrar.Location = new System.Drawing.Point(509, 94);
            this.btnBorrar.Name = "btnBorrar";
            this.btnBorrar.Size = new System.Drawing.Size(75, 23);
            this.btnBorrar.TabIndex = 74;
            this.btnBorrar.Text = "Borrar";
            this.btnBorrar.UseVisualStyleBackColor = true;
            this.btnBorrar.Click += new System.EventHandler(this.btnBorrar_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(370, 381);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(95, 23);
            this.button2.TabIndex = 75;
            this.button2.Text = "Cargar Txt";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 476);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnBorrar);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.txAgregarBase);
            this.Controls.Add(this.lbxBases);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.txHoraEnvio2);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.txHoraEnvio1);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.cbxMailEnvio);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.txMailPuerto);
            this.Controls.Add(this.txMailPassword);
            this.Controls.Add(this.txMailUsuario);
            this.Controls.Add(this.txMailSmtp);
            this.Controls.Add(this.txMailEnvia);
            this.Controls.Add(this.txOPGetEstado);
            this.Controls.Add(this.cbxOPGetEstado);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.txSistemaSAP);
            this.Controls.Add(this.txPassBase);
            this.Controls.Add(this.txUsuarioBase);
            this.Controls.Add(this.txVersionSQL);
            this.Controls.Add(this.txPassSAP);
            this.Controls.Add(this.txUsuarioSAP);
            this.Controls.Add(this.txServidorLic);
            this.Controls.Add(this.txServidor);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSalir);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Datos Configuración";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txServidor;
        private System.Windows.Forms.TextBox txServidorLic;
        private System.Windows.Forms.TextBox txUsuarioSAP;
        private System.Windows.Forms.TextBox txPassSAP;
        private System.Windows.Forms.TextBox txVersionSQL;
        private System.Windows.Forms.TextBox txUsuarioBase;
        private System.Windows.Forms.TextBox txPassBase;
        private System.Windows.Forms.TextBox txSistemaSAP;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.ComboBox cbxOPGetEstado;
        private System.Windows.Forms.TextBox txOPGetEstado;
        private System.Windows.Forms.TextBox txMailEnvia;
        private System.Windows.Forms.TextBox txMailSmtp;
        private System.Windows.Forms.TextBox txMailUsuario;
        private System.Windows.Forms.TextBox txMailPassword;
        private System.Windows.Forms.TextBox txMailPuerto;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ComboBox cbxMailEnvio;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox txHoraEnvio1;
        private System.Windows.Forms.TextBox txHoraEnvio2;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.ListBox lbxBases;
        private System.Windows.Forms.TextBox txAgregarBase;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Button btnBorrar;
        private System.Windows.Forms.Button button2;
    }
}


﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Mapping;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjetoAcademia
{
    public partial class F_GestaoTurmas : Form
    {
        string idSelecionado;
        int modo = 0; // 0 = padrão, 1 = edição , 2 = inserção
        string vqueryDGV;
        public F_GestaoTurmas()
        {
            InitializeComponent();
        }

        private void F_GestaoTurmas_Load(object sender, EventArgs e)
        {
            vqueryDGV = @"
                SELECT 
                    tbt.N_IDTURMA as 'ID',
                    tbt.T_DSCTURMA as 'Turma',
                    tbh.T_DSCHORARIO as 'Horario' 
                FROM 
                    tb_turmas as tbt
                INNER JOIN 
                    tb_horarios as tbh on tbh.N_IDHORARIO = tbt.N_IDHORARIO
            ";
            dgv_turmas.DataSource = Banco.dql(vqueryDGV);
            dgv_turmas.Columns[0].Width = 40    ;
            dgv_turmas.Columns[1].Width = 120;
            dgv_turmas.Columns[2].Width = 85;

            /* Popular combobox professores */

            string vqueryProfessores = @"
                SELECT 
                    N_IDPROFESSOR,
                    T_NOMEPROFESSOR 
                FROM
                    tb_professores 
                ORDER BY 
                    N_IDPROFESSOR
            ";

            cb_professor.Items.Clear(); // sempre limpar antes de iniciar
            cb_professor.DataSource = Banco.dql(vqueryProfessores);
            cb_professor.DisplayMember = "T_NOMEPROFESSOR";
            cb_professor.ValueMember = "N_IDPROFESSOR";

            /* Popular Combobox Status (Ativa = A, Paralisada = P, Cancelada = C) */

            Dictionary<string, string> st = new Dictionary<string, string>();
            st.Add("A", "Ativa"); // A = Key -- Ativa == Value 
            st.Add("P", "Paralisada");
            st.Add("C", "Cancelada");
            cb_status.Items.Clear();//limpar antes de criar
            cb_status.DataSource = new BindingSource(st, null);
            cb_status.DisplayMember = "Value";
            cb_status.ValueMember = "Key";

            /* Popular Combobox Horarios */

            string vqueryHorarios = @"
                SELECT 
                    * 
                FROM
                    tb_horarios 
                ORDER BY 
                    T_DSCHORARIO
            ";

            cb_horario.Items.Clear(); // sempre limpar antes de iniciar
            cb_horario.DataSource = Banco.dql(vqueryHorarios);
            cb_horario.DisplayMember = "T_DSCHORARIO";
            cb_horario.ValueMember = "N_IDHORARIO";

            /**/
        }

        private void dgv_turmas_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int contLinhas = dgv.SelectedRows.Count;
            if(contLinhas > 0)
            {
                modo = 0;
                idSelecionado = dgv_turmas.Rows[dgv_turmas.SelectedRows[0].Index].Cells[0].Value.ToString();
                string vqueryCampos = @"
                    SELECT 
                        T_DSCTURMA, 
                        N_IDPROFESSOR,
                        N_IDHORARIO,
                        N_MAXALUNOS,
                        T_STATUS
                    FROM
                        tb_turmas
                    WHERE 
                        N_IDTURMA ="+idSelecionado;
                DataTable dt = Banco.dql(vqueryCampos);
                tb_dscturma.Text = dt.Rows[0].Field<string>("T_DSCTURMA");
                cb_professor.SelectedValue = dt.Rows[0].Field<Int64>("N_IDPROFESSOR").ToString();
                n_maxAlunos.Value = dt.Rows[0].Field<Int64>("N_MAXALUNOS");
                cb_status.SelectedValue = dt.Rows[0].Field<string>("T_STATUS");
                cb_horario.SelectedValue = dt.Rows[0].Field<Int64>("N_IDHORARIO");

            }
        }

        private void btn_novaturma_Click(object sender, EventArgs e)
        {
            tb_dscturma.Clear();
            cb_professor.SelectedIndex = -1;
            n_maxAlunos.Value = 0;
            cb_status.SelectedIndex = -1;
            cb_horario.SelectedIndex = -1;
            tb_dscturma.Focus();
            modo = 2;
        }

        private void btn_salvaredicoes_Click(object sender, EventArgs e)
        {
            
            if (modo != 0) {
                string queryTurma = "";
                string msg = "";
                if (modo == 1) {
                    msg = "Dados alterados";
                    queryTurma = String.Format(@"
                    UPDATE
                        tb_turmas 
                    SET 
                        T_DSCTURMA = '{0}',
                        N_IDPROFESSOR = {1},
                        N_IDHORARIO = {2},
                        N_MAXALUNOS = {3},
                        T_STATUS = '{4}'
                    WHERE 
                        N_IDTURMA = {5}", tb_dscturma.Text, cb_professor.SelectedValue, cb_horario.SelectedValue, Int32.Parse(Math.Round(n_maxAlunos.Value, 0).ToString()), cb_status.SelectedValue, idSelecionado);

                }
                else
                {
                    msg = "nova turma inserida";
                    queryTurma = String.Format(@"
                        INSERT INTO tb_turmas 
                        (T_DSCTURMA,N_IDPROFESSOR,N_IDHORARIO,N_MAXALUNOS,T_STATUS)
                        VALUES('{0}',{1},{2},{3},'{4}')", tb_dscturma.Text, cb_professor.SelectedValue, cb_horario.SelectedValue, Int32.Parse(Math.Round(n_maxAlunos.Value, 0).ToString()), cb_status.SelectedValue);
                }

                int linha = dgv_turmas.SelectedRows[0].Index;
                Banco.dml(queryTurma);
                if(modo == 1)
                {
                    dgv_turmas[1, linha].Value = tb_dscturma.Text;
                    dgv_turmas[2, linha].Value = cb_horario.Text;
                }
                else
                {
                    dgv_turmas.DataSource = Banco.dql(vqueryDGV);
                }
                MessageBox.Show(msg);

            }
        }

        private void btn_excluirturma_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Confirma exclusão?", "Excluir?", MessageBoxButtons.YesNo);
            if(res == DialogResult.Yes)
            {
                string queryExcluirTurma = String.Format(@" 
                    DELETE 
                    FROM 
                        tb_turmas
                    WHERE
                        N_IDTURMA={0}", idSelecionado);
                Banco.dml(queryExcluirTurma);
                dgv_turmas.Rows.Remove(dgv_turmas.CurrentRow);
            }
        }

        private void btn_fechar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tb_dscturma_TextChanged(object sender, EventArgs e)
        {
            if (modo == 0)
            {
                modo = 1;
            }
        }

        private void cb_professor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modo == 0)
            {
                modo = 1;
            }
        }

        private void n_maxAlunos_ValueChanged(object sender, EventArgs e)
        {
            if (modo == 0)
            {
                modo = 1;
            }
        }

        private void cb_status_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modo == 0)
            {
                modo = 1;
            }
        }

        private void cb_horario_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modo == 0)
            {
                modo = 1;
            }
        }
    }
}

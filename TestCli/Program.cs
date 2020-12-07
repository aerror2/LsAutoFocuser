using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using LsStepMotor;
using System.Threading;
namespace TestLinShengStepperMotor
{
    class Program
    {


        static void print_bytes(byte[] resp, int nr)
        {
            StringBuilder sb = new StringBuilder();
            for(int i=0;i<nr;i++)
            {
                sb.Append(String.Format(" {0:X2}", resp[i])); 
            }
            Console.WriteLine(sb.ToString());
        }
        static uint getCurPos(SerialPort sp)
        {
            byte[] resp = new byte[7];

            sp.Write(LinShengStepMotor.cmd_query_curpos, 0, LinShengStepMotor.cmd_query_curpos.Length);
            int nr = 0;
            while (nr != resp.Length && sp.IsOpen)
            {
                int tn = sp.Read(resp, nr, resp.Length - nr);
                if (tn <= 0)
                    return 0;
                nr += tn;
            }
            print_bytes(resp,nr);
            uint curpos = LinShengStepMotor.extrace_query_response_value_0_3(resp);
            return curpos;
        }

        static uint getSetPos(SerialPort sp)
        {
            byte[] resp = new byte[7];

            sp.Write(LinShengStepMotor.cmd_query_setpos, 0, LinShengStepMotor.cmd_query_curpos.Length);
            int nr = 0;
            while (nr != resp.Length && sp.IsOpen)
            {
                int tn = sp.Read(resp, nr, resp.Length - nr);
                if (tn <= 0)
                    return 0;
                nr += tn;
            }
            print_bytes(resp, nr);
            uint pos = LinShengStepMotor.extrace_query_response_value_0_3(resp);
            return pos;
        }

        static SpeedInfo getCurrentSpeed(SerialPort sp)
        {
            byte[] resp = new byte[6];

            sp.Write(LinShengStepMotor.cmd_query_cur_speed, 0, LinShengStepMotor.cmd_query_curpos.Length);
            int nr = 0;
            while (nr != resp.Length && sp.IsOpen)
            {
                int tn = sp.Read(resp, nr, resp.Length - nr);
                if (tn <= 0)
                    return new SpeedInfo();
                nr += tn;
            }
            print_bytes(resp, nr);
            return LinShengStepMotor.extrace_query_response_speed(resp);
        }

        static void waitForFinish(SerialPort myPort)
        {
            uint curpos = 0;
            uint setpos = 0;
            SpeedInfo spd = getCurrentSpeed(myPort);
            for (int i = 0; i < 400; i++)
            {

                curpos = getCurPos(myPort);
                setpos = getSetPos(myPort);
                //spd = getCurrentSpeed(myPort);

                Console.WriteLine("即时运行位置:" + curpos + " 设定运行位置:" + setpos + " 速度倍数:" + spd.level + " 速度单位毫秒:" + spd.speed);


                if (curpos == setpos)
                {
                    Console.WriteLine("到达目标位置");
                    break;
                }

                uint wt = (setpos>curpos?(setpos-curpos):(curpos-setpos))* spd.speed / 1000;
                Thread.Sleep((int)wt);

            }


        }

        public static void moveAndWaitStep(bool add, uint nstep, SerialPort port)
        {

            byte[] cmd = add ? LinShengStepMotor.cmd_acc_add_relative_position(nstep) :
                LinShengStepMotor.cmd_acc_sub_relative_position(nstep);

            port.Write(cmd, 0, cmd.Length);

            waitForFinish(port);

        }
            
         static void Main(string[] args)
        {
            uint curpos = 0;
            uint setpos = 0;
            SpeedInfo spd = null;
            SerialPort myPort = new SerialPort();
            byte[] cmd = null;

            myPort.PortName = "COM7";
            myPort.BaudRate = 9600;
            myPort.Open();
            //myPort.DataReceived += myPort_DataReceived;
            Console.WriteLine("连接完成");



            Console.WriteLine("设置 EQ ");
            myPort.Write(LinShengStepMotor.cmd_reset_main, 0, LinShengStepMotor.cmd_reset_eq.Length);

            
           
            spd = getCurrentSpeed(myPort);
   
            curpos = getCurPos(myPort);
            setpos = getSetPos(myPort);
            if (curpos != 0)
            {
                moveAndWaitStep(false, curpos, myPort);
            }
            Console.WriteLine("即时运行位置:" + curpos + " 设定运行位置:" + setpos + " 速度倍数:" + spd.level + " 速度单位毫秒:" + spd.speed);

            uint myspeed = 4000;
            if (spd.speed != myspeed)
            {
                Console.WriteLine("修改速度: " + myspeed);
                cmd = LinShengStepMotor.cmd_set_speed_ex(0, (ushort)(myspeed));
                myPort.Write(cmd, 0, cmd.Length);
                Thread.Sleep(1000);
            }
            //spd = getCurrentSpeed(myPort);
            //curpos = getCurPos(myPort);
            //setpos = getSetPos(myPort);
            //Console.WriteLine("即时运行位置:" + curpos + " 设定运行位置:" + setpos + " 速度倍数:" + spd.level + " 速度单位毫秒:" + spd.speed);

            Console.WriteLine("转轴位置累加 4096 ");


            moveAndWaitStep(true, 5500, myPort);



       //     Console.WriteLine("转轴位置累减 4096 ");
       //     moveAndWaitStep(false, 4096, myPort);

          

            Console.ReadLine();
            myPort.Close();

        }

      
    }
}

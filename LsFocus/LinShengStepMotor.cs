using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsStepMotor
{
     public class SpeedInfo
    {
        public uint level;
        public uint speed;
    };

    public class LinShengStepMotor
    {

        public static byte[] cmd_stop = new byte[] { 0, 1, 0, 0, 0, 0 };  //Stop  停止

        public static byte[] cmd_mov_clockwise = new byte[] { 0, 2, 0, 0, 0, 0 }; //00 02 00 00 00 00  正转  
        public static byte[] cmd_mov_conterclockwise = new byte[] { 0, 3, 0, 0, 0, 0 };//00 03 00 00 00 00  反转
        public static byte[] cmd_reset_main = new byte[] { 0, 4, 0, 0, 0, 0 }; //主预置
        public static byte[] cmd_speed_down = new byte[] { 0, 5, 0, 0, 0, 0 }; //
        public static byte[] cmd_speed_up = new byte[] { 0, 6, 0, 0, 0, 0 };
        public static byte[] cmd_reset_eq = new byte[] { 0, 9, 0, 0, 0, 0 }; //EQ 预置
        public static byte[] cmd_clear_position = new byte[] { 0, 0x32, 0, 0, 0, 0 }; //0x32 清除位置     ：nn 32 00 00 00 00  //功能位置信息全部为0
        public static byte[] cmd_align_position = new byte[] { 0, 0x33, 0, 0, 0, 0 }; // 0x33 直接对齐位置；nn 33 xx xx xx xx  //xx为对齐位置 电机不会运行
        public static byte[] cmd_reset_factory = new byte[] { 0, 0x34, 0x63, 0, 0, 0 }; //00 34 63 00 00 00  恢复出厂值代码（从新通电起效）
        public static byte[] cmd_reset_limit_swith = new byte[] { 0, 0x3f, 0x00, 0, 0, 0 };//0x3f 复位码 nn 3f 00 00 00 00       //电机碰到复位开关后反弹停止

        public static byte[] cmd_set_speed_ratio(byte level)//00 45 00 xx 00 00  速度等级  
        {
            byte[] ret = new byte[] { 0, 0x45, 0, 0, 0, 0 };
            ret[3] = (byte)level;
            return ret;
        }

        public static byte[] cmd_lock_speed(byte param)//00 4a 00 xx 00 00  转轴锁定xx=01锁定 00不锁定02-4 锁定一段时间释放
        {
            byte[] ret = new byte[] { 0, 0x4a, 0, 0, 0, 0 };
            ret[3] = (byte)param;
            return ret;
        }


        public static byte[] cmd_set_com_address(byte addr) //00 52 00 xx 00 00  串口地址码修改
        {
            byte[] ret = new byte[] { 0, 0x52, 0, 0, 0, 0 };
            ret[3] = (byte)addr;
            return ret;
        }

        public static byte[] cmd_set_speed_ex(byte level, ushort speed)// 速度细控  //2020年04月03号改为00 57 00 nn xx xx  ；nn是速度等级,(0-255) xxxx是速度值 xx xx （0-49000(BF68) ）
        {
            byte[] ret = new byte[] { 0, 0x57, 0, 0, 0, 0 };
            ret[3] = (byte)level;
            ret[4] = (byte)(speed >> 8 & 0xff);
            ret[5] = (byte)(speed & 0xff);

            return ret;
        }

        public static void uint2parambytes(byte[] ret, int idx, uint val)
        {
            ret[idx + 0] = (byte)(val >> 24 & 0xff);
            ret[idx + 1] = (byte)(val >> 16 & 0xff);
            ret[idx + 2] = (byte)(val >> 8 & 0xff);
            ret[idx + 3] = (byte)(val & 0xff);
        }

        public static byte[] cmd_set_abs_position(uint pos)//00 58 xx xx xx xx  直接输入转轴位置
        {
            byte[] ret = new byte[] { 0, 0x58, 0, 0, 0, 0 };
            uint2parambytes(ret, 2, pos);
            return ret;
        }
        public static byte[] cmd_acc_add_relative_position(uint pos)//00 59 xx xx xx xx  转轴位置累加
        {
            byte[] ret = new byte[] { 0, 0x59, 0, 0, 0, 0 };
            uint2parambytes(ret, 2, pos);
            return ret;
        }

        public static byte[] cmd_acc_sub_relative_position(uint pos)//00 5a xx xx xx xx  转轴位置累减
        {
            byte[] ret = new byte[] { 0, 0x5a, 0, 0, 0, 0 };
            uint2parambytes(ret, 2, pos);
            return ret;
        }

        public static byte[] cmd_force_move_clockwise_steps(uint pos) //00 5b xx xx xx xx  强制正转步数
        {
            byte[] ret = new byte[] { 0, 0x5b, 0, 0, 0, 0 };
            uint2parambytes(ret, 2, pos);
            return ret;
        }

        public static byte[] cmd_force_move_counterclockwise_steps(uint pos) //00 5c xx xx xx xx  强制反转步数
        {
            byte[] ret = new byte[] { 0, 0x5c, 0, 0, 0, 0 };
            uint2parambytes(ret, 2, pos);
            return ret;
        }

        public static uint extrace_query_response_value_0_3(byte[] resp)
        {
            uint ret = 0;
            ret |= ((uint)resp[2]) << 24 & 0xff000000;
            ret |= ((uint)resp[3]) << 16 & 0xff0000;
            ret |= ((uint)resp[4]) << 8 & 0xff00;
            ret |= ((uint)resp[5]) & 0xff;
            return ret;
        }

        public static uint extrace_query_response_value_4_5(byte[] resp)
        {
            return resp[2];
        }


        public static SpeedInfo extrace_query_response_speed(byte[] resp)
        {
            SpeedInfo ret = new SpeedInfo();
            ret.level = resp[2];
            ret.speed = ((uint)resp[3] << 8 & 0xff00) | (uint)resp[4];
            return ret;
        }


        //查询指令格式：nn 67 xx 00 00 00  //nn地址码 xx查询项目 
        //查询项目0到3返回格式：nn FD xx xx xx xx FC //左往右 数据高到低
        //查询项目4和5返回格式：nn FD xx FD   //nn地址码 fc标记 xx返回数值 
        // 查询指令功能
        //0 即时运行位置
        //1 设定运行位置
        //2 程控负循环次数
        //3 程控主循环次数
        //4 程控负指令指针
        //5 程控主指令指针
        //  6 当前速度        //返回格式nn FD yy xx xx FD   yy是速度倍数 xx 是速度单位毫秒 速度最终值=(yy+1)*xxxx

        public static byte[] cmd_query_curpos = new byte[] { 0, 0x67, 0, 0, 0, 0 };
        public static byte[] cmd_query_setpos = new byte[] { 0, 0x67, 1, 0, 0, 0 };
        public static byte[] cmd_query_negative_loop_count = new byte[] { 0, 0x67, 2, 0, 0, 0 };
        public static byte[] cmd_query_main_loop_count = new byte[] { 0, 0x67, 3, 0, 0, 0 };
        public static byte[] cmd_query_negative_pointer = new byte[] { 0, 0x67, 4, 0, 0, 0 };
        public static byte[] cmd_query_main_pointer = new byte[] { 0, 0x67, 5, 0, 0, 0 };
        public static byte[] cmd_query_cur_speed = new byte[] { 0, 0x67, 6, 0, 0, 0 };


        //response format 
        // nn   fc  xx  fc   littel endian 
        // port tag value       tag 

        //   0x68 //发送格式：nn 68 xx xx  yy 00  //xx是查询起始地址 yy是查询长度 
        //返回格式：nn fe yy... fe   //nn地址码 fe是标识 yy是返回数据长度和接收到的指令数值一样
        //多个控制板串口串联 地址自动编码指令
        //0x69 //格式：nn 69 63 xx 00 00  ；nn地址码（一般设为1为宜）  xx起始编号

    }
}

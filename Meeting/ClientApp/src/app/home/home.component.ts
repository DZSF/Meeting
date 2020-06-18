import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EChartOption } from 'echarts';
import { TimeUtil } from '../common/common.util';

class IRoomItem {
  room: string;
  id: string;
  option: EChartOption;
  info: string[];
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.less']
})
export class HomeComponent {
  public user: string = '';
  public selectedDate = null;
  public templateOption: EChartOption = {};
  public listOfRoomItem: IRoomItem[];

  public modalVisible: boolean = false;
  public modalRoom: string;
  public modalStartTime: string;
  public modalEndTIme: string;
  public listOfTime: string[] = [
    '09:00', '09:30', '10:00', '10:30', '11:00', '11:30', '12:00', '12:30', '13:00', '13:30',
    '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00', '17:30', '18:00', '18:30',
    '19:00', '19:30', '20:00', '20:30', '21:00'
  ];

  private _baseUrl: string;
  private _http: HttpClient;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this._http = http;
    this._baseUrl = baseUrl + 'api/meeting/home/';
    this._getInitial();
    this._initialTemplateOption();
  }

  public dateSelected(selectedDate: Date): void {
    let httpParams: { [key: string]: string } = { 'date': TimeUtil.dateFormat(selectedDate.getTime(), 'yyyy-MM-dd') };
    let paramJson: string = JSON.stringify(httpParams);
    this._http.get<any>(this._baseUrl + 'schedulelist', { params: { paramJson } }).subscribe(result => {
      if (result) {
        this._drawScheduleCharts(result);
      }
    }, error => console.error(error));
  }

  public popupToSchedule(room: string): void {
    this.modalVisible = true;
    this.modalRoom = room;
  }

  public modalOk(): void {
    this.modalVisible = false;
    let body: any = {
      room: this.modalRoom,
      user: this.user ? this.user : "张媛媛",
      start: this.modalStartTime,
      end: this.modalEndTIme,
      date: TimeUtil.dateFormat(this.selectedDate.getTime(), 'yyyy-MM-dd')
    };
    this.modalStartTime = null;
    this.modalEndTIme = null;
    this._http.post(this._baseUrl + 'book/', body).subscribe(result => {
      console.log('Book conference status: ' + result);
      if (result) { // need test
        alert('Book conference ok.');
        this.dateSelected(this.selectedDate);
      } else {
        alert('Book failed.');
      }
    }, error => console.error(error));
  }

  public modalCancel(): void {
    this.modalVisible = false;
  }

  private _getInitial(): void {
    this._http.get<any>(this._baseUrl + 'initial').subscribe(result => {
      this.user = result.user;
    }, error => console.error(error));
  }

  private _initialTemplateOption(): void {
    let xData: string[] = this.listOfTime;
    let seriesData: number[] = [];
    for (let i in xData) {
      seriesData.push(0);
    }
    this.templateOption = {
      color: ["green"],
      grid: {
        top: '25'
      },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        splitLine: {
          show: true,
          interval: 0
        },
        data: xData
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: { show: false },
        axisLabel: {
          formatter: function () {
            return '';
          }
        }
      },
      series: [{
        data: seriesData,
        type: 'line',
        symbol: 'circle',
        symbolSize: 5
      }]
    };
  }

  private _drawScheduleCharts(data: any): void {
    let list: IRoomItem[] = [];
    let option = {
      color: ['green', 'green'],
      grid: {
        top: '25'
      },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        splitLine: {
          show: true
        },
        axisLabel: {
          formatter: function () {
            return '';
          }
        },
        data: this.listOfTime
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: { show: false },
        axisLabel: {
          formatter: function () {
            return '';
          }
        }
      },
      series: [{
        data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0],
        type: 'line',
        symbol: 'circle',
        symbolSize: 5
      },
      {
        data: [],
        type: 'line',
        areaStyle: {},
        symbol: 'none'
      }
      ]
    };
    for (let room of Object.keys(data)) {
      let item: IRoomItem = new IRoomItem();
      item.room = room;
      item.info = data[room];
      item.option = option;

      let listOfData: any[] = [];
      for (let scheduleItem of item.info) {
        listOfData.push([scheduleItem["Start"], 1], [scheduleItem["End"], 1]);
      }
      item.option.series[1].data = listOfData;
      list.push(item);
    }
    this.listOfRoomItem = list.slice(0);
  }
}

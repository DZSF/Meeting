import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EChartOption } from 'echarts';

class IRoomItem {
  room: string;
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
    this._getUser();
    this._initialTemplateOption();
  }

  public dateSelected(result: Date): void {
    console.log('select date: ' + result);
    let httpParams: { [key: string]: string } = { 'date': result.toString(), 'test': 'testValue' };
    this._http.get<any>(this._baseUrl + 'schedulelist', { params: httpParams }).subscribe(result => {
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
      user: this.user,
      start: this.listOfTime.indexOf(this.modalStartTime),
      end: this.listOfTime.indexOf(this.modalEndTIme),
      date: this.selectedDate
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

  private _getUser(): void {
    this._http.get<any>(this._baseUrl + 'user').subscribe(result => {
      this.user = result;
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
        data: [
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
          10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
          21, 22, 23, 24
        ]
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

      // ['1/zhangyuanyuan@1-2,16-18', '/Jack@3-5']
      let listOfTimeInterval: string[] = [];
      for (let str of item.info) {
        listOfTimeInterval = listOfTimeInterval.concat(str.split('@')[1].split(','));
      }
      // ['1-2', '16-18', '3-5'] -> ['1-2', '3-5','16-18']
      listOfTimeInterval.sort(this._sortFunc);
      let listOfData: any[] = [];
      for (let timeInterval of listOfTimeInterval) {
        let arr: string[] = timeInterval.split('-');
        let start: number = Number(arr[0]);
        let end: number = Number(arr[1]);
        listOfData.push([start, 0], [start, 1], [end, 1], [end, 0]);
      }
      // [[1, 0], [1, 1], [2, 1], [2, 0], [3, 0], [3, 1], [5, 1], [5, 0], [16, 0], [16, 1], [18, 1], [18, 0]]
      item.option.series[1].data = listOfData;
      list.push(item);
    }
    this.listOfRoomItem = list.slice(0);
  }

  private _sortFunc(val1: string, val2: string): number {
    return Number(val1.split('-')[0]) - Number(val2.split('-')[0]);
  }
}

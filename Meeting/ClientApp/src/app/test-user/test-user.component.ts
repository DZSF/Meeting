import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-test-user',
  templateUrl: './test-user.component.html',
  styleUrls: ['./test-user.component.less']
})
export class TestUserComponent implements OnInit {
  public user = '';
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<string>(baseUrl + 'api/testuser').subscribe(result => {
      if (result && result.length !== 0) {
        this.user = result[0];
      }
    }, error => console.error(error));
  }

  ngOnInit(): void {
  }

}

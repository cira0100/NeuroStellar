import { Component, OnInit } from '@angular/core';
import { DatasetsService } from 'src/app/_services/datasets.service';
import Dataset from 'src/app/_data/Dataset';
import {Router} from '@angular/router'
import { JwtHelperService } from '@auth0/angular-jwt';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-filter-datasets',
  templateUrl: './filter-datasets.component.html',
  styleUrls: ['./filter-datasets.component.css']
})
export class FilterDatasetsComponent implements OnInit {

  publicDatasets?: Dataset[];
  term: string = "";
  constructor(private datasets: DatasetsService,private router:Router, private cookie: CookieService) {
    this.datasets.getPublicDatasets().subscribe((datasets) => {
      this.publicDatasets = datasets;
    });
  }

  ngOnInit(): void {

  }
  addDataset(dataset: Dataset):void{
    //this.router.navigateByUrl('/predict?id='+id);
    const helper = new JwtHelperService();
    const decodedToken = helper.decodeToken(this.cookie.get("token"));
    dataset._id = "";
    dataset.isPublic = false;
    dataset.lastUpdated = new Date();
    dataset.username = decodedToken.name;
    this.datasets.addDataset(dataset).subscribe((response:string)=>{
      console.log(response);
    });
  };

}

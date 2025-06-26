import { Component } from '@angular/core';
import { PublicHeaderComponent } from "../public-header/public-header.component";

@Component({
  selector: 'app-landing-page',
  imports: [PublicHeaderComponent],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.css'
})
export class LandingPageComponent {

}

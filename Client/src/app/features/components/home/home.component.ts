import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import { SidebarComponent } from "../../../shared/components/sidebar/sidebar.component";
import { PrivateHeaderComponent } from "../../../shared/components/private-header/private-header.component";
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { UserService } from '../../../core/services/user/user.service';
import { catchError, of, Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [SidebarComponent, PrivateHeaderComponent, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit { 
  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
  ){}

  ngOnInit(): void {
    const role = localStorage.getItem('role');
    if (role === 'Admin'){
      this.router.navigate(['./dashboard'], { relativeTo: this.activatedRoute });
    }
    else if (role === 'Customer'){
      this.router.navigate(['./book-appointment'], { relativeTo: this.activatedRoute });
    }
    else{
      this.router.navigate(['/unauthorized']);
    }
  }
} 
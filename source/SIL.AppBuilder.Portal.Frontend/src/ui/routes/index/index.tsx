import * as React from 'react';
import { DWKitForm } from 'vendor/dwkit/optimajet-form';

export default class IndexRoute extends React.Component {
  state = { data: {}, errors: {} };
  render() {
    return (
      <div>
        <h2>Index Route </h2>

        <DWKitForm
          eventFunc={console.log}
          formName='login'
          modelurl='/ui/login'
          data={this.state.data}
          errors={this.state.errors}
        />
      </div>
    )
  }
}

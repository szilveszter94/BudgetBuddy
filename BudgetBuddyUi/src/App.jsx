import { Route, Routes } from "react-router-dom";
import Home from "./components/Home/Home";
import Account from "./components/Account/Account";
import Transaction from "./components/Transaction/Transaction";
import Achievement from "./components/Achievement/Achievement";
import TransactionCreator from "./components/Create/TransactionCreator/TransactionCreator";
import AchievementCreator from "./components/Create/AchievementCreator/AchievementCreator";
import AccountCreator from "./components/Create/AccountCreator/AccountCreator";
import Login from "./components/Authentication/Login/Login";
import Register from "./components/Authentication/Register/Register";
import UserProfile from "./components/UserProfile/UserProfile";
import Reports from "./components/Report/Reports";
import ReportDetails from "./components/Report/ReportDetails";
import ReportCreator from "./components/Create/ReportCreator/ReportCreator";

const App = () => {
  return (
    <div>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/account/:id" element={<Account />} />
        <Route path="/transaction/:id" element={<Transaction />} />
        <Route path="/achievement/:id" element={<Achievement />} />
        <Route path="/account/create" element={<AccountCreator />} />
        <Route path="/transaction/create" element={<TransactionCreator />} />
        <Route path="/achievement/create" element={<AchievementCreator />} />
        <Route path="account/update/:id" element={<AccountCreator />} />
        <Route path="transaction/update/:id" element={<TransactionCreator />} />
        <Route path="achievement/update/:id" element={<AchievementCreator />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/profile/:id" element={<UserProfile />} />
        <Route path="/reports/" element={<Reports />} />
        <Route path="/reports/:reportId" element={<ReportDetails />} />
        <Route path="/reports/add" element={<ReportCreator />} />
      </Routes>
    </div>
  );
};

export default App;
